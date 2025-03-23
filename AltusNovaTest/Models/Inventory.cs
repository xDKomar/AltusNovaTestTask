using System.ComponentModel.DataAnnotations;

namespace AltusNovaTest.Models;

public class Inventory
{
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public ICollection<OrderItem> Orders { get; set; } = new List<OrderItem>();
        public byte[] Version { get; set; } = null!;
}