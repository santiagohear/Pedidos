using Microsoft.EntityFrameworkCore;
using Api.Entities;
using Api.Infrastructure;
using Api.Dtos;
using Api.Services;

namespace Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<int> CreateOrderAsync(CreateOrderDto dto)
    {
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != productIds.Count) throw new InvalidOperationException("Uno o más productos no existen");

        foreach (var it in dto.Items)
        {
            if (it.Quantity <= 0) throw new InvalidOperationException("La cantidad debe ser mayor a cero");
            var prod = products.First(p => p.Id == it.ProductId);
            if (prod.Stock < it.Quantity) throw new InvalidOperationException($"No hay suficiente stock para el producto {prod.Id}");
        }

        var order = new Order { CustomerName = dto.CustomerName };
        foreach (var it in dto.Items)
        {
            var prod = products.First(p => p.Id == it.ProductId);
            prod.Stock -= it.Quantity;
            var oi = new OrderItem { ProductId = prod.Id, Quantity = it.Quantity, UnitPrice = prod.Price };
            order.Items.Add(oi);
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        return order.Id;
    }
}