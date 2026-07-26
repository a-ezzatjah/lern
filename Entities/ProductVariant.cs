using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product product { get; set; }

        public int ProductSaleOptionId { get; set; }
        public ProductSaleOption ProductSaleOption { get; set; }

        public int ProductSaleOptionColorId { get; set; }
        public ProductSaleOptionColor ProductSaleOptionColor { get; set; } = null!;


        // اطلاعات انبارداری و قیمت
        public string Sku { get; set; } // شناسه انبارداری یکتا (مثلا FABRIC-METRIC-RED)
        public decimal Price { get; set; } // قیمت این تنوع خاص
        public decimal? DiscountValue { get; set; }
        
        public DisconTypeEnum? DisconType { get; set; }
        public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }

        public int StockQuantity { get; set; } // موجودی واقعی فیزیکی در انبار
        public int ReservedQuantity { get; set; } // موجودی رزرو شده (مثلاً در سبد خریدهای معلق)
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal Step { get; set; } = 1;

        public int AvailableQuantity => StockQuantity - ReservedQuantity;


        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();




    }
}
