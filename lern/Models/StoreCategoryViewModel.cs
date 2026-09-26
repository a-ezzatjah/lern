using Entities;
using ServiceContract.Common;
using ServiceContract.DTO.DtoProduct;

namespace lern.Models;

public sealed record StoreCategoryViewModel(
    Category Category,
    IReadOnlyList<Category> Breadcrumbs,
    IReadOnlyList<Category> Children,
    PageResult<ProductCardDto> Products);
