using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // اختیاری: اگر عکس متعلق به یک رنگ یا تنوع خاص است
        public int? VariantId { get; set; }
        public ProductVariant Variant { get; set; }

        public string ImageUrl { get; set; }
        public string AltText { get; set; }

        public int SortOrder { get; set; } // برای مشخص کردن ترتیب نمایش در گالری (0, 1, 2...)
        public bool IsPrimary { get; set; } // آیا عکس اصلی/کارت محصول است؟











    }
}
