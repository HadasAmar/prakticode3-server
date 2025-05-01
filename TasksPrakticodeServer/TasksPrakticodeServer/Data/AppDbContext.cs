using Microsoft.EntityFrameworkCore;
using TasksPrakticodeServer.Models;

namespace TasksPrakticodeServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Item> Items => Set<Item>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
            modelBuilder.Entity<Customer>().Property(c => c.Email).HasMaxLength(100);
            modelBuilder.Entity<Customer>().Property(c => c.Name).HasMaxLength(50);
            modelBuilder.Entity<Customer>().Property(c => c.Password).HasMaxLength(20);

            modelBuilder.Entity<Item>().Property(i => i.Name).HasMaxLength(50);
        }
    }
}



