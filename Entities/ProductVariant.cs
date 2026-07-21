using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product product { get; set; }

        public int ProductSaleOptionId { get; set; }
        public ProductSaleOption ProductSaleOption { get; set; }

        public int SaleOptionColorId { get; set; }
        public SaleOptionColor optionColor { get; set; }


        // اطلاعات انبارداری و قیمت
        public string Sku { get; set; } // شناسه انبارداری یکتا (مثلا FABRIC-METRIC-RED)
        public decimal Price { get; set; } // قیمت این تنوع خاص
        public int StockQuantity { get; set; } // موجودی واقعی فیزیکی در انبار
        public int ReservedQuantity { get; set; } // موجودی رزرو شده (مثلاً در سبد خریدهای معلق)



        public int AvailableQuantity => StockQuantity - ReservedQuantity;






    }
}
