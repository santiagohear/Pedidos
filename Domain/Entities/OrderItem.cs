using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }
    }
}