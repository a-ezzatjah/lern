using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public decimal Price { get; set; }
        public string Slug { get; set; } = null!;

        public string ?Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ProductCategory> productCategories { get; set; } = new List<ProductCategory>();  //چرا پیش فرض کلاس

        public ICollection<ProductSaleOption> SaleOptions { get; set; } = new();  //چرا پیش فرض کلاس

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public SeoData? Seo { get; set; }

    }
}
