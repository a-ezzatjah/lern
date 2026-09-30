using ServiceContract.DTO.DtoProduct;
using Entities;

namespace lern.Models;

public class HomeIndexViewModel
{
    public List<SiteBanner> Banners { get; set; } = [];
    public List<ProductCardDto> DiscountedProducts { get; set; } = new();
    public List<ProductCardDto> NewestProducts { get; set; } = new();
}
