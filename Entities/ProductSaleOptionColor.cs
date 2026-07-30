using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class ProductSaleOptionColor
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Color { get; set; } = null!;

        public int ProductSaleOptionId { get; set; }
        public ProductSaleOption ProductSaleOption { get; set; } = null!;

        public string? HexCode { get; set; }

        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}
