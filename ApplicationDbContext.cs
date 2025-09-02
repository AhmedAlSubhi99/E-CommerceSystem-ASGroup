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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                        .HasIndex(u => u.Email)
                        .IsUnique();
            // One Category -> Many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // One Supplier -> Many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.Status)
                 .HasConversion<string>()
                 .HasMaxLength(50)
                 .IsRequired();
                e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            // If Product has a decimal price, it’s good to fix its precision too ... 
            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.OverallRating).HasPrecision(18, 2);
            });
            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.Status)
                 .HasConversion<string>()                 // store as nvarchar
                 .HasMaxLength(20)
                 .HasDefaultValue(OrderStatus.Pending);

                e.Property(o => o.TotalAmount)
                 .HasPrecision(18, 2);                    // (fixes your EF warning)

                e.HasIndex(o => o.Status);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.Property(p => p.OverallRating).HasPrecision(4, 2); // fixes the other EF warning
                e.Property(p => p.Price).HasPrecision(18, 2);
            });
        }
    }
}
