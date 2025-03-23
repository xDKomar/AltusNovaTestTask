using AltusNovaTest.Data;
using AltusNovaTest.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AltusNovaTest;

public static class Endpoints
{
        public static void MapEndpoints(this WebApplication app)
        {
                var orderGroup = app.MapGroup("/orders");
                orderGroup.MapGet("/{id}", GetOrderByIdHandler);
                orderGroup.MapGet("/{id}/status", GetOrderStatusHandler);
                orderGroup.MapPut("/{id}/confirm", ConfirmOrderHandler);
                orderGroup.MapPost("/", CreateOrdersHandler);
        }

        public static async Task<IResult> GetOrderByIdHandler(int id, NovaContext db)
        {
                var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
                return order == null ? Results.NotFound() : Results.Ok(order);
        }

        public static async Task<IResult> GetOrderStatusHandler(int id, NovaContext db)
        {
                var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                        return Results.NotFound();
                }

                return Results.Ok(new { OrderStatus = (OrderStatus)order.StatusId });
        }

        public static async Task<IResult> ConfirmOrderHandler(int id, NovaContext db)
        {
                //There is a requirement to implement inventory checks before confirming an order
                //Looks ambiguous to me, because there is also a requirement to prevent incorrect stock reservations at the moment of placement,
                //which suppose that stock is reserved at the moment of placement.
                //Need to clarify
                var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                        return Results.NotFound();
                }

                if (order.StatusId != (int)OrderStatus.Pending)
                {
                        return Results.BadRequest(new
                        {
                                Error = "Only pending orders can be confirmed"
                        });
                }

                order.StatusId = (int)OrderStatus.Confirmed;
                await db.SaveChangesAsync();

                return Results.Ok(order);
        }

        public static async Task<IResult> CreateOrdersHandler(List<Order> orders, NovaContext db, IValidator<Order> validator)
        {
                var errors = new List<object>();
                foreach (var order in orders)
                {
                        var validationResult = await validator.ValidateAsync(order);
                        if (!validationResult.IsValid)
                        {
                                errors.Add(new { order.OrderNumber, validationResult.Errors });
                                continue;
                        }

                        if (errors.Any())
                        {
                                continue;
                        }

                        order.StatusId = (int)OrderStatus.Pending;
                        order.OrderDate = DateTime.UtcNow;
                        order.OrderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}";

                        foreach (var item in order.Items)
                        {
                                var product = await db.Inventory.FindAsync(item.ProductId);
                                item.UnitPrice = product!.UnitPrice;
                                item.Amount = item.UnitPrice * item.Quantity;
                                product.Quantity -= item.Quantity;
                        }

                        order.TotalAmount = order.Items.Sum(i => i.Amount);
                        db.Orders.Add(order);
                }

                if (errors.Any())
                {
                        return Results.BadRequest(new { Success = false, Errors = errors });
                }

                try
                {
                        await db.SaveChangesAsync();
                        return Results.Ok(new { Success = true });
                }
                catch (Exception ex)
                {
                        return Results.BadRequest(new { Success = false, Error = ex.Message });
                }
        }
}