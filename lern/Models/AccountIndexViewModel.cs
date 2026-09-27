using Entities;
using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public sealed class AccountIndexViewModel
{
    public int FavoriteCount { get; init; }
    public List<ProductCardDto> RecentlyViewedProducts { get; init; } = new();
    public List<Order> RecentOrders { get; init; } = new();
    public int ActiveOrderCount { get; init; }
    public int ShippedOrderCount { get; init; }
}
