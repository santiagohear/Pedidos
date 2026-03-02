using Api.Services;
using Microsoft.EntityFrameworkCore;
using Api.Entities;
using Api.Infrastructure;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

// configure DB from connection string; fall back to InMemory when not provided
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connection))
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("PedidosDb"));
}
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Products.Any())
    {
        db.Products.Add(new Product { Name = "Product A", Price = 10m, Stock = 100 });
        db.Products.Add(new Product { Name = "Product B", Price = 20m, Stock = 50 });
        db.SaveChanges();
    }
}

app.Run();