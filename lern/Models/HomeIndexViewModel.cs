using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public class HomeIndexViewModel
{
    public List<ProductCardDto> DiscountedProducts { get; set; } = new();
}
