using System.ComponentModel.DataAnnotations;

namespace AltusNovaTest.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public int StatusId { get; set; }
    public OrderStatus Status => (OrderStatus)StatusId;
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Shipped = 3
} 