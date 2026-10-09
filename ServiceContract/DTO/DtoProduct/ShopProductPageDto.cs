using ServiceContract.Common;

namespace ServiceContract.DTO.DtoProduct;

public sealed record ShopColorDto(string Name, string? HexCode);

public sealed class ShopProductPageDto : PageResult<ProductCardDto>
{
    public IReadOnlyList<ShopColorDto> Colors { get; init; } = [];
    public IReadOnlySet<string> SelectedColors { get; init; } = new HashSet<string>();
}
