using ServiceContract.DTO.DtoProduct;
namespace lern.Models;
public sealed class FavoritesViewModel
{
    public List<ProductCardDto> Products { get; init; } = new();
    public List<ProductCardDto> Suggested { get; init; } = new();
    public Dictionary<int, FavoriteCartOption> CartOptions { get; init; } = new();
    public Dictionary<int, string> Categories { get; init; } = new();
    public Dictionary<int, int[]> ProductCategories { get; init; } = new();
}

public record FavoriteCartOption(int VariantId, int Quantity);
