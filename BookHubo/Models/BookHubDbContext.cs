using Microsoft.EntityFrameworkCore;

namespace BookHubo.Models
{
    public class BookHubDbContext : DbContext
    {
        public BookHubDbContext(DbContextOptions<BookHubDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Email phải unique
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Book indexes
            modelBuilder.Entity<Book>().HasIndex(b => b.SellerId);
            modelBuilder.Entity<Book>().HasIndex(b => b.Category);
            modelBuilder.Entity<Book>().HasIndex(b => b.IsActive);

            // Order indexes
            modelBuilder.Entity<Order>().HasIndex(o => o.BuyerId);

            // OrderItem indexes
            modelBuilder.Entity<OrderItem>().HasIndex(oi => oi.SellerId);
            modelBuilder.Entity<OrderItem>().HasIndex(oi => oi.Status);

            // CartItem indexes
            modelBuilder.Entity<CartItem>().HasIndex(ci => ci.UserId);
            modelBuilder.Entity<CartItem>().HasIndex(ci => ci.BookId);
        }
    }
}
