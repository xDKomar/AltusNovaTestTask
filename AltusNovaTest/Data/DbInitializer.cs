using AltusNovaTest.Models;

namespace AltusNovaTest.Data;

public static class DbInitializer
{
    public static void Initialize(NovaContext context)
    {
        context.Database.EnsureCreated();

        if (context.Inventory.Any())
        {
            return;
        }

        var inventoryItems = new Inventory[]
        {
            new Inventory
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Quantity = 10,
                UnitPrice = 999.99m,
                Category = "Electronics",
                Version = [1, 2, 3, 4, 5 ]
            },
            new Inventory
            {
                Name = "Smartphone",
                Description = "Latest model smartphone",
                Quantity = 20,
                UnitPrice = 699.99m,
                Category = "Electronics",
                Version = [1, 2, 3, 4, 5 ]
            },
            new Inventory
            {
                Name = "Headphones",
                Description = "Wireless noise-cancelling headphones",
                Quantity = 15,
                UnitPrice = 199.99m,
                Category = "Accessories",
                Version = [1, 2, 3, 4, 5 ]
            }
        };

        context.Inventory.AddRange(inventoryItems);
        context.SaveChanges();

        var orders = new Order[]
        {
            new Order
            {
                OrderNumber = "ORD001",
                OrderDate = DateTime.UtcNow,
                StatusId = (int)OrderStatus.Pending,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = inventoryItems[0].Id,
                        Quantity = 2,
                        UnitPrice = inventoryItems[0].UnitPrice,
                        Amount = inventoryItems[0].UnitPrice * 2
                    },
                    new OrderItem
                    {
                        ProductId = inventoryItems[1].Id,
                        Quantity = 1,
                        UnitPrice = inventoryItems[1].UnitPrice,
                        Amount = inventoryItems[1].UnitPrice
                    }
                }
            },
            new Order
            {
                OrderNumber = "ORD002",
                OrderDate = DateTime.UtcNow,
                StatusId = (int)OrderStatus.Confirmed,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = inventoryItems[2].Id,
                        Quantity = 3,
                        UnitPrice = inventoryItems[2].UnitPrice,
                        Amount = inventoryItems[2].UnitPrice * 3
                    }
                }
            }
        };

        foreach (var order in orders)
        {
            order.TotalAmount = order.Items.Sum(i => i.Amount);
        }

        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
} 