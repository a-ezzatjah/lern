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

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSaleOption> ProductSaleOptions { get; set; }
        public DbSet<SaleOptionColor> SaleOptionColors { get; set; }

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
                .HasOne(x => x.Product)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.ProductId);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryId);

            // Category self-referencing
            modelBuilder.Entity<Category>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

// Product → SeoData (Owned Entity)
            modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Seo, seo =>
                {
                   
                    seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                    seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                    seo.Property(s => s.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                    seo.Property(s => s.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                    seo.Property(s => s.OgTitle).HasMaxLength(200).HasColumnName("OgTitle");
                    seo.Property(s => s.OgDescription).HasMaxLength(500).HasColumnName("OgDescription");
                    seo.Property(s => s.OgImageUrl).HasMaxLength(500).HasColumnName("OgImageUrl");
                    seo.Property(s => s.IndexPage).HasColumnName("IndexPage");
                    seo.Property(s => s.FollowPage).HasColumnName("FollowPage");
                });


            // Category → SeoData (Owned Entity)

            modelBuilder.Entity<Category>()
               .OwnsOne(c => c.Seo, seo =>
               {
                   seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                   seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                   seo.Property(s => s.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                   seo.Property(s => s.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                   seo.Property(s => s.OgTitle).HasMaxLength(200).HasColumnName("OgTitle");
                   seo.Property(s => s.OgDescription).HasMaxLength(500).HasColumnName("OgDescription");
                   seo.Property(s => s.OgImageUrl).HasMaxLength(500).HasColumnName("OgImageUrl");
                   seo.Property(s => s.IndexPage).HasColumnName("IndexPage");
                   seo.Property(s => s.FollowPage).HasColumnName("FollowPage");
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
                .WithMany(pso => pso.SaleOptionColors)
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