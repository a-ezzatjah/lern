using Entities;
using ServiceContract.Common;
using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public sealed record ShopViewModel(PageResult<ProductCardDto> Products, IReadOnlyList<Category> Categories,
    IReadOnlyList<Category> Breadcrumbs, string Query, IReadOnlySet<int> SelectedCategories,
    bool AvailableOnly, string Sort, decimal? MinPrice, decimal? MaxPrice, bool DiscountedOnly = false);

public sealed record ShopCategoryNodeViewModel(Category Category, IReadOnlyList<Category> Categories,
    IReadOnlySet<int> SelectedCategories, IReadOnlySet<int> ExpandedCategories, int Depth = 0);
