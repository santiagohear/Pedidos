using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Infrastructure;
using Api.Entities;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0 || pageSize <= 0) return BadRequest(new { error = "los valores de paginación deben ser mayor o igual a cero" });

        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(p => p.Name.Contains(name));

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound(new { error = "Producto no existe" });
        return Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Product input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _db.Products.Add(input);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Product input)
    {
        if (id != input.Id) return BadRequest(new { error = "Los Id's son diferentes" });
        var existing = await _db.Products.FindAsync(id);
        if (existing == null) return NotFound(new { error = "Producto no existe" });

        existing.Name = input.Name;
        existing.Price = input.Price;
        existing.Stock = input.Stock;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Products.FindAsync(id);
        if (existing == null) return NotFound(new { error = "Producto no existe" });

        var referenced = await _db.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (referenced) return Conflict(new { error = "El producto existe en una orden, no se puede eliminar" });

        _db.Products.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
