using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Enums;

namespace Entities
{
    public class ProductSaleOption
    {

        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Title { get; set; } = null!;
        public EnumSaleType SaleType { get; set; }


        public string? UnitName { get; set; }
        public string? InputLabel { get; set; }




        public ICollection<SaleOptionColor>? SaleOptionColors { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();


    }


}

