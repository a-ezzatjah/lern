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
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategories");
            modelBuilder.Entity<ProductSaleOption>().ToTable("ProductSaleOptions");
            modelBuilder.Entity<ProductSaleOptionColor>().ToTable("ProductSaleOptionColors");
            modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");

            modelBuilder.Entity<ProductCategory>()
                .HasKey(x => new { x.ProductId, x.CategoryId });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Product)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.ProductCategories)
                .HasForeignKey(x => x.CategoryId);

            modelBuilder.Entity<Category>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductSaleOption>()
                .HasOne(x => x.Product)
                .WithMany(x => x.SaleOptions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductSaleOptionColor>()
                .HasOne(x => x.ProductSaleOption)
                .WithMany(x => x.ProductSaleOptionColors)
                .HasForeignKey(x => x.ProductSaleOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(x => x.product)
                .WithMany(x => x.productVariants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(x => x.ProductSaleOption)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductSaleOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(x => x.ProductSaleOptionColor)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductSaleOptionColorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductImage>()
                .HasOne(x => x.Product)
                .WithMany(x => x.productImages)
                .HasForeignKey(x => x.ProductId);

            modelBuilder.Entity<ProductImage>()
                .HasOne(x => x.Variant)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.VariantId);

            modelBuilder.Entity<Product>()
                .OwnsOne(x => x.Seo, seo =>
                {
                    seo.Property(x => x.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                    seo.Property(x => x.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                    seo.Property(x => x.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                    seo.Property(x => x.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                    seo.Property(x => x.OgTitle).HasMaxLength(200).HasColumnName("OgTitle");
                    seo.Property(x => x.OgDescription).HasMaxLength(500).HasColumnName("OgDescription");
                    seo.Property(x => x.OgImageUrl).HasMaxLength(500).HasColumnName("OgImageUrl");
                    seo.Property(x => x.IndexPage).HasColumnName("IndexPage");
                    seo.Property(x => x.FollowPage).HasColumnName("FollowPage");
                });

            modelBuilder.Entity<Category>()
                .OwnsOne(x => x.Seo, seo =>
                {
                    seo.Property(x => x.MetaTitle).HasMaxLength(200).HasColumnName("MetaTitle");
                    seo.Property(x => x.MetaDescription).HasMaxLength(500).HasColumnName("MetaDescription");
                    seo.Property(x => x.MetaKeywords).HasMaxLength(300).HasColumnName("MetaKeywords");
                    seo.Property(x => x.CanonicalUrl).HasMaxLength(500).HasColumnName("CanonicalUrl");
                    seo.Property(x => x.OgTitle).HasMaxLength(200).HasColumnName("OgTitle");
                    seo.Property(x => x.OgDescription).HasMaxLength(500).HasColumnName("OgDescription");
                    seo.Property(x => x.OgImageUrl).HasMaxLength(500).HasColumnName("OgImageUrl");
                    seo.Property(x => x.IndexPage).HasColumnName("IndexPage");
                    seo.Property(x => x.FollowPage).HasColumnName("FollowPage");
                });

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.ImageUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<ProductSaleOptionColor>()
                .Property(x => x.ImageUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<ProductSaleOptionColor>()
                .Property(x => x.HexCode)
                .HasMaxLength(20);

            modelBuilder.Entity<Product>()
                .Property(x => x.DiscountValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.MinQuantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.MaxQuantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.Step)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.FixedWeight)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.FixedLength)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.FixedWidth)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.FixedHeight)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.PerUnitWeight)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.PerUnitLength)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.PerUnitWidth)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOption>()
                .Property(x => x.PerUnitHeight)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductSaleOptionColor>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(x => x.DiscountValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(x => x.MinQuantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductVariant>()
                .Property(x => x.MaxQuantity)
                .HasPrecision(18, 3);

            modelBuilder.Entity<ProductVariant>()
                .Property(x => x.Step)
                .HasPrecision(18, 3);

            modelBuilder.Entity<Product>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(x => x.Slug)
                .IsUnique();
        }
    }
}
