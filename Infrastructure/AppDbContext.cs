using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(b =>
            {
                b.Property(p => p.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
                b.HasOne(oi => oi.Product).WithMany().HasForeignKey(oi => oi.ProductId);
                b.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId);
            });
        }
    }
}