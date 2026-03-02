using System.ComponentModel.DataAnnotations;

namespace Api.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string CustomerName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<OrderItem> Items { get; set; } = new();
    }
}