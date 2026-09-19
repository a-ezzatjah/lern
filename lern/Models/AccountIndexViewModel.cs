using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public sealed class AccountIndexViewModel
{
    public List<ProductCardDto> RecentlyViewedProducts { get; init; } = new();
}
