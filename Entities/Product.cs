using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace Entities
{
    public class Product
    {

        [Key]

        public int Id { get; set; }
      
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }

        public string? ShortDescription { get; set; }
        public bool IsActive { get; set; } = true;

        [Column("Discount")]
        public decimal? DiscountValue { get; set; }

        [Column("DisconType")]
        public DisconTypeEnum? DiscountType { get; set; }

        public DateTime? DiscountStartAt { get; set; }

        public DateTime? DiscountEndAt { get; set; }


        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<ProductSaleOption> SaleOptions { get; set; } = new List<ProductSaleOption>();

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public SeoData? Seo { get; set; }

    }
}
