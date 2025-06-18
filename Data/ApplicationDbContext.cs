using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ElectronicsStore.Models; // Assuming your models are in this namespace

namespace ElectronicsStore.Data
{
    // ApplicationDbContext inherits from IdentityDbContext for user management
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSet for your application's entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TrafficLog> TrafficLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base method first to configure Identity tables
            base.OnModelCreating(modelBuilder);

            // Configure decimal precisions
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            // Configure relationships and delete behaviors as seen in your migration
            // Product - Category (Many-to-One, Category can be null, Product delete sets CategoryId to null)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrderItem - Order (Many-to-One, OrderItem delete cascades from Order)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem - Product (Many-to-One, OrderItem delete restricts Product deletion)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // No navigation property on Product back to OrderItem
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Important: Prevents deleting a product if it's part of an order item

            // ProductImage - Product (Many-to-One, ProductImage delete cascades from Product)
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review - Product (Many-to-One, Review delete cascades from Product)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review - ApplicationUser (Many-to-One, Review delete restricts User deletion)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany() // No navigation property on ApplicationUser back to Review
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - ApplicationUser (Many-to-One, Order delete cascades from User)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() // No navigation property on ApplicationUser back to Order
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure other model properties (max lengths, etc.)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
                entity.Property(e => e.ShortDescription).IsRequired().HasMaxLength(500);
                entity.Property(e => e.MainImageUrl).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(250);
                entity.Property(e => e.PhoneNumber).IsRequired(); 
                entity.Property(e => e.PaymentMethod).IsRequired();
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
            });
        }
    }
}