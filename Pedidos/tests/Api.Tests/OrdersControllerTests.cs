using Api.Controllers;
using Api.Dtos;
using Api.Entities;
using Api.Infrastructure;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Api.Tests;

public class OrdersControllerTests
{
    private AppDbContext CreateDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDbContext(opt);
        db.Products.Add(new Product { Name = "P1", Price = 10m, Stock = 10 });
        db.Products.Add(new Product { Name = "P2", Price = 5m, Stock = 3 });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task CreateOrder_ShouldDecreaseStock()
    {
        var db = CreateDb();
        var svc = new OrderService(db);
        var ctrl = new OrdersController(db, svc);

        var dto = new CreateOrderDto("C", new List<CreateOrderItemDto> { new CreateOrderItemDto(db.Products.First().Id, 2) });
        var res = await ctrl.Post(dto);
        var prod = db.Products.First();
        Assert.Equal(8, prod.Stock);
    }

    [Fact]
    public async Task DeleteOrder_ShouldRestoreStock()
    {
        var db = CreateDb();
        var svc = new OrderService(db);
        var ctrl = new OrdersController(db, svc);

        var dto = new CreateOrderDto("C", new List<CreateOrderItemDto> { new CreateOrderItemDto(db.Products.First().Id, 2) });
        var post = await ctrl.Post(dto);
        var created = (post as Microsoft.AspNetCore.Mvc.CreatedAtActionResult)!;
        var order = created.Value as Order;
        Assert.NotNull(order);

        // delete
        await ctrl.Delete(order!.Id);
        var prod = db.Products.First();
        Assert.Equal(10, prod.Stock);
    }
}
