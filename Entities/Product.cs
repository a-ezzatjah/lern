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
      
        public string Name { get; set; }
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        [Column("Discount")]
        public decimal? DiscountValue { get; set; }

        [Column("DisconType")]
        public DisconTypeEnum? DiscountType { get; set; }

        [NotMapped]
        public bool HasDiscount => DiscountValue.HasValue && DiscountValue.Value > 0;

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public ICollection<ProductSaleOption> SaleOptions { get; set; } = new List<ProductSaleOption>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public SeoData? Seo { get; set; }

    }
}
