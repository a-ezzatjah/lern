using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {


        }

        public DbSet<Product> products { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<ProductCategory> productCategories { get; set; }
        public DbSet<ProductSaleOption> productSaleOptions { get; set; }
        public DbSet<SaleOptionColor> saleOptionColors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategories");
            modelBuilder.Entity<ProductSaleOption>().ToTable("ProductSaleOptions");
            modelBuilder.Entity<SaleOptionColor>().ToTable("SaleOptionColors");

            // ProductCategory composite key
            modelBuilder.Entity<ProductCategory>().HasKey(x => new { x.ProductId, x.CategoryId });
            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.product)
                .WithMany(x => x.productCategories)
                .HasForeignKey(x => x.ProductId);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.productCategories)
                .HasForeignKey(x => x.CategoryId);

            // Category self-referencing
            modelBuilder.Entity<Category>()
                .HasOne(x => x.parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → SeoData (Owned Entity)
            modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Seo, seo =>
                {
                    seo.ToTable("ProductSeoData");
                    seo.Property(s => s.Id).HasColumnName("Id");
                    seo.Property(s => s.Title).HasMaxLength(200);
                    seo.Property(s => s.Description).HasMaxLength(500);
                    seo.Property(s => s.Keywords).HasMaxLength(300);
                    seo.Property(s => s.CanonicalUrl).HasMaxLength(500);
                });

            // Product → ProductSaleOption
            modelBuilder.Entity<ProductSaleOption>()
                .HasOne(pso => pso.Product)
                .WithMany(p => p.SaleOptions)
                .HasForeignKey(pso => pso.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductSaleOption → SaleOptionColor
            modelBuilder.Entity<SaleOptionColor>()
                .HasOne(soc => soc.ProductSaleOption)
                .WithMany(pso => pso.saleOptionColors)
                .HasForeignKey(soc => soc.SaleOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product constraints
            modelBuilder.Entity<Product>()
                .Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>()
                .Property(p => p.Slug).IsRequired().HasMaxLength(200);

            // Category constraints
            modelBuilder.Entity<Category>()
                .Property(c => c.Name).IsRequired().HasMaxLength(100);
        }


    }
}