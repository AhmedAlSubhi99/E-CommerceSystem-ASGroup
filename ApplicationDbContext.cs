using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProducts> OrderProducts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>()
        .HasOne(r => r.User)
        .WithMany(u => u.RefreshTokens)
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product
            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.OverallRating).HasPrecision(18, 2);
                e.Property(p => p.StockQuantity).HasDefaultValue(0);
            });

            // Order
            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasDefaultValue(OrderStatus.Pending)
                    .IsRequired();

                e.Property(o => o.TotalAmount).HasPrecision(18, 2);
                e.HasIndex(o => o.Status);
            });


            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.OverallRating)
                .HasColumnType("decimal(3,2)");
        }

    }
}
