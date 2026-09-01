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
        public DbSet<ProductSaleOptionColor> ProductSaleOptionColors { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages {get;set;}
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategories");
            modelBuilder.Entity<ProductSaleOption>().ToTable("ProductSaleOptions");
            modelBuilder.Entity<ProductSaleOptionColor>().ToTable("SaleOptionColors");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImage");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<PaymentTransaction>().ToTable("PaymentTransactions");
            modelBuilder.Entity<CartItem>().HasIndex(x => new { x.CustomerKey, x.ProductVariantId }).IsUnique();
            modelBuilder.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CartItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CartItem>().HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentTransaction>().HasOne(x => x.Order).WithMany(x => x.Transactions).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);



            // ProductCategory composite key
            modelBuilder.Entity<ProductCategory>().HasKey(x => new { x.ProductId, x.CategoryId });
            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Product)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            // Category self-referencing
            modelBuilder.Entity<Category>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);



            // ProductSaleOption
            modelBuilder.Entity<ProductSaleOption>()
                .HasOne(pso => pso.Product)
                .WithMany(p => p.SaleOptions)
                .HasForeignKey(pso => pso.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            


            // ProductSaleOptionColor
            modelBuilder.Entity<ProductSaleOptionColor>()
             .HasOne(soc => soc.ProductSaleOption)
             .WithMany(pso => pso.SaleOptionColors)
             .HasForeignKey(soc => soc.ProductSaleOptionId)
             .OnDelete(DeleteBehavior.Cascade);


            // ProductVariant
           
            modelBuilder.Entity<ProductVariant>()
                .HasOne(x => x.ProductSaleOption)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductSaleOptionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductVariant>()
                .HasOne(x => x.saleoptioncolor)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductSaleOptionColorId);




            // ProductImage
            modelBuilder.Entity<ProductImage>()
                .HasOne(x => x.Product)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductImage>()
                .HasOne(x => x.Variant)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Restrict);







            // Product â†’ SeoData (Owned Entity)
            modelBuilder.Entity<Product>()
                .OwnsOne(p => p.Seo, seo =>
                {
                   
                    seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                    seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                    seo.Property(s => s.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                    seo.Property(s => s.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                    seo.Property(s => s.IndexPage).HasColumnName("IndexPage");
                    seo.Property(s => s.FollowPage).HasColumnName("FollowPage");
                });


            // Category â†’ SeoData (Owned Entity)

            modelBuilder.Entity<Category>()
               .OwnsOne(c => c.Seo, seo =>
               {
                   seo.Property(s => s.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                   seo.Property(s => s.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                   seo.Property(s => s.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                   seo.Property(s => s.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                   seo.Property(s => s.IndexPage).HasColumnName("IndexPage");
                   seo.Property(s => s.FollowPage).HasColumnName("FollowPage");
               });




           


            // Decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.DiscountValue).HasPrecision(18, 2);
            modelBuilder.Entity<ProductSaleOption>()
                .Property(pso => pso.MinQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<ProductSaleOption>()
                .Property(pso => pso.MaxQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<ProductSaleOption>()
                .Property(pso => pso.Step).HasPrecision(18, 3);
             
            // ProductSaleOption → ProductSaleOptionColor
     

            modelBuilder.Entity<ProductSaleOptionColor>()
                .Property(soc => soc.HexCode)
                .HasMaxLength(20);

         

            // Product constraints
            modelBuilder.Entity<Product>()
                .Property(p => p.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>()
                .Property(p => p.Slug).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug).IsUnique();

            // Category constraints
            modelBuilder.Entity<Category>()
                .Property(c => c.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name).IsUnique();
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug).IsUnique();














        }


    }
}
