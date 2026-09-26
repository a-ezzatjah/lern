using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public sealed class AccountIndexViewModel
{
    public int FavoriteCount { get; init; }
    public List<ProductCardDto> RecentlyViewedProducts { get; init; } = new();
}
