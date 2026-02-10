using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)  
        {
        }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Supplier Configuration
            modelBuilder.Entity<Supplier>(entity =>
            {
            entity.HasKey(s => s.SupplierId);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.ContactPerson).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Phone).IsRequired().HasMaxLength(20);
            entity.Property(s => s.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(s => s.ModifiedDate).HasDefaultValueSql("GETDATE()");
            
            entity.HasIndex(s => s.Name);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
                entity.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.ModifiedDate).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(p => p.SKU).IsUnique();
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Category);

                entity.HasOne(p => p.Supplier)
                        .WithMany(p => p.Products)
                        .HasForeignKey(p => p.SupplierId)
                        .OnDelete(DeleteBehavior.SetNull);
            });

            // StockMovement configuration
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(sm => sm.MovementId);
                entity.Property(sm => sm.MovementType).IsRequired()
                        .HasConversion<string>()
                        .HasMaxLength(10);
                entity.Property(sm => sm.CreatedDate).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(sm => sm.ProductId);
                entity.HasIndex(sm => sm.MovementDate);
                entity.HasIndex(sm =>sm.MovementType);

                entity.HasOne(sm => sm.Product)
                        .WithMany(p => p.StockMovements)
                        .HasForeignKey(sm => sm.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(

                new Supplier
                {
                    SupplierId = 1,
                    Name = "Tech Distributors Inc.",
                    ContactPerson = "John Smith",
                    Email = "john.smith@techdist.com",
                    Phone = "+1-555-0101",
                    Address = "123 Tech Ave, Silicon Valley, CA 94025",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Supplier
                {
                    SupplierId = 2,
                    Name = "Office Supplies Co.",
                    ContactPerson = "Sarah Johnson",
                    Email = "sarah.j@officesupplies.com",
                    Phone = "+1-555-0102",
                    Address = "456 Business Blvd, New York, NY 10001",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Supplier
                {
                    SupplierId = 3,
                    Name = "Global Electronics Ltd.",
                    ContactPerson = "Michael Chen",
                    Email = "m.chen@globalelec.com",
                    Phone = "+1-555-0103",
                    Address = "789 Electronics Way, Austin, TX 78701",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Wireless Mouse",
                    SKU = "WM-001",
                    Description = "Ergonomic wireless mouse with USB receiver",
                    Category = "Computer Accessories",
                    UnitPrice = 29.99m,
                    CurrentStock = 50,
                    LowStockThreshold = 10,
                    SupplierId = 1,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Mechanical Keyboard",
                    SKU = "KB-002",
                    Description = "RGB mechanical gaming keyboard",
                    Category = "Computer Accessories",
                    UnitPrice = 89.99m,
                    CurrentStock = 30,
                    LowStockThreshold = 5,
                    SupplierId = 1,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 3,
                    Name = "USB-C Cable",
                    SKU = "CBL-003",
                    Description = "6ft USB-C to USB-C cable",
                    Category = "Cables",
                    UnitPrice = 12.99m,
                    CurrentStock = 100,
                    LowStockThreshold = 20,
                    SupplierId = 3,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 4,
                    Name = "A4 Printer Paper",
                    SKU = "PPR-004",
                    Description = "500 sheets, 80gsm white paper",
                    Category = "Office Supplies",
                    UnitPrice = 8.99m,
                    CurrentStock = 5,
                    LowStockThreshold = 15,
                    SupplierId = 2,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 5,
                    Name = "Ballpoint Pens (Box of 12)",
                    SKU = "PEN-005",
                    Description = "Blue ink ballpoint pens",
                    Category = "Office Supplies",
                    UnitPrice = 5.99m,
                    CurrentStock = 25,
                    LowStockThreshold = 10,
                    SupplierId = 2,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            );
        }

    }

}
