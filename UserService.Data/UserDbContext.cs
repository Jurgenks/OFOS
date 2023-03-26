using Microsoft.EntityFrameworkCore;
using User = OFOS.Domain.Models.User;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}