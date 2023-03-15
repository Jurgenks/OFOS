using Microsoft.EntityFrameworkCore;
using OFOS.Domain.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Delivery> Delivery { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
