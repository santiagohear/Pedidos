using Api.Controllers;
using Api.Entities;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Api.Tests;

public class ProductsControllerTests
{
    private AppDbContext CreateDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDbContext(opt);
        db.Products.Add(new Product { Name = "P1", Price = 10m, Stock = 10 });
        db.Products.Add(new Product { Name = "P2", Price = 5m, Stock = 0 });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Get_ShouldReturnProducts()
    {
        var db = CreateDb();
        var ctrl = new ProductsController(db);
        var res = await ctrl.Get(null);
        Assert.NotNull(res);
    }

    [Fact]
    public async Task Delete_ReferencedProduct_ShouldReturnConflict()
    {
        var db = CreateDb();
        // add order referencing product 1
        var order = new Order { CustomerName = "c" };
        order.Items.Add(new OrderItem { ProductId = db.Products.First().Id, Quantity = 1, UnitPrice = 10m });
        db.Orders.Add(order);
        db.SaveChanges();

        var ctrl = new ProductsController(db);
        var result = await ctrl.Delete(db.Products.First().Id);
        Assert.IsType<Microsoft.AspNetCore.Mvc.ConflictObjectResult>(result);
    }
}
