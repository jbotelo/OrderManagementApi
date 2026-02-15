using Microsoft.EntityFrameworkCore;
using Orm.Domain.Entities;

namespace Orm.Infrastructure.DbContexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().ToTable(nameof(Order));

            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderID);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems);

            modelBuilder.Entity<OrderItem>().ToTable(nameof(OrderItem));

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.OrderItemID);
        }
    }
}
