using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public class HomeIndexViewModel
{
    public List<ProductCardDto> DiscountedProducts { get; set; } = new();
    public List<ProductCardDto> NewestProducts { get; set; } = new();
}
