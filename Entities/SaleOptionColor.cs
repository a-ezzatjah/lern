using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SaleOptionColor
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Color { get; set; } = null!;

        public int SaleOptionId { get; set; }
        public ProductSaleOption ProductSaleOption { get; set; } = null!;

        public decimal Price { get; set; }  // قیمت مخصوص این رنگ

        public string? HexCode { get; set; }
    }
}