using AltusNovaTest.Data;
using AltusNovaTest.Models;
using AltusNovaTest.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AltusNovaTest.Tests;

public class OrderTests
{
        private readonly NovaContext _context;

        public OrderTests()
        {
                DbContextOptions<NovaContext> options = new DbContextOptionsBuilder<NovaContext>().UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid()).Options;
                _context = new NovaContext(options);
        }

        [Fact]
        public async Task CreateOrder_WithValidData_ShouldSucceed()
        {
                var product = new Inventory
                {
                        Id = 1,
                        Quantity = 10,
                        Name = "Sample Product",
                        UnitPrice = 100.0m,
                        Description = "A sample product for testing",
                        Version = [1, 2, 3, 4, 5]
                };
                _context.Inventory.Add(product);
                await _context.SaveChangesAsync();

                var order = new Order
                {
                        OrderNumber = "1234567890",
                        OrderDate = DateTime.Now,
                        StatusId = (int)OrderStatus.Pending,
                        TotalAmount = 200.0m,
                        Items = new List<OrderItem>
                        {
                                new()
                                {
                                        ProductId = 1,
                                        Quantity = 2
                                }
                        }
                };

                var result = await Endpoints.CreateOrdersHandler([order], _context, new OrderValidator(_context));
                var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
                Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);

                var savedOrder = await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);

                Assert.NotNull(savedOrder);
                Assert.Equal(2, savedOrder.Items.First().Quantity);
                Assert.Equal(200, savedOrder.TotalAmount);
                Assert.Equal((int)OrderStatus.Pending, savedOrder.StatusId);

                var updatedProduct = await _context.Inventory.FindAsync(1);
                Assert.NotNull(updatedProduct);
                Assert.Equal(8, updatedProduct.Quantity);
        }

        [Fact]
        public async Task CreateOrder_WithInsufficientStock()
        {
                var product = new Inventory
                {
                        Id = 1,
                        Quantity = 10,
                        Name = "Sample Product",
                        UnitPrice = 100.0m,
                        Description = "A sample product for testing",
                        Version = [1, 2, 3, 4, 5]
                };
                _context.Inventory.Add(product);
                await _context.SaveChangesAsync();

                var order = new Order
                {
                        Items = new List<OrderItem>
                        {
                                new OrderItem
                                {
                                        ProductId = 1,
                                        Quantity = 15
                                }
                        }
                };

               var result = await Endpoints.CreateOrdersHandler([order], _context, new OrderValidator(_context));
               var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
               Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);

                var savedOrder = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == order.Id);
                Assert.Null(savedOrder);
                
                var updatedProduct = await _context.Inventory.FindAsync(1);
                Assert.NotNull(updatedProduct);
                Assert.Equal(10, updatedProduct.Quantity);
        }

        [Fact]
        public async Task ConfirmOrder_WithValidPendingOrder()
        {
                var order = new Order
                {
                        OrderNumber = "ORD123",
                        OrderDate = DateTime.UtcNow,
                        StatusId = (int)OrderStatus.Pending,
                        TotalAmount = 100.0m,
                        Items = new List<OrderItem>
                        {
                                new()
                                {
                                        ProductId = 1,
                                        Quantity = 1,
                                        UnitPrice = 100.0m,
                                        Amount = 100.0m
                                }
                        }
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var result = await Endpoints.ConfirmOrderHandler(order.Id, _context);
                var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
                Assert.Equal(StatusCodes.Status200OK, statusResult.StatusCode);

                var confirmedOrder = await _context.Orders.FindAsync(order.Id);
                Assert.NotNull(confirmedOrder);
                Assert.Equal((int)OrderStatus.Confirmed, confirmedOrder.StatusId);
        }

        [Fact]
        public async Task ConfirmOrder_WithNonExistentOrder()
        {
                var result = await Endpoints.ConfirmOrderHandler(999, _context);
                var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
                Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
        }
}