using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Infrastructure;
using Api.Dtos;
using Api.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrderService _orders;

    public OrdersController(AppDbContext db, IOrderService orders) => (_db, _orders) = (db, orders);

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _db.Orders.Include(o => o.Items).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound(new { error = "Orden no existe" });
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName) || dto.CustomerName.Length > 120)
            return BadRequest(new { error = "Nombre incorrecto" });

        if (dto.Items == null || !dto.Items.Any()) return BadRequest(new { error = "La orden debe contener items" });

        try
        {
            var id = await _orders.CreateOrderAsync(dto);
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            return CreatedAtAction(nameof(Get), new { id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound(new { error = "Orden no existe" });

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        foreach (var it in order.Items)
        {
            var prod = products.First(p => p.Id == it.ProductId);
            prod.Stock += it.Quantity;
        }

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
