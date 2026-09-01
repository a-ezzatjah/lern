using System.Globalization;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.DTO.DtoProductVariant;

namespace lern.Models;

public class StoreProductDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? Description { get; init; }
    public List<ProductGalleryImageViewModel> Gallery { get; init; } = new();
    public List<ProductOptionViewModel> Options { get; init; } = new();
    public ProductVariantOptionViewModel? SelectedVariant { get; init; }

    public string PrimaryImageUrl =>
        Gallery.FirstOrDefault()?.ImageUrl ?? "/assets/images/placeholder.png";

    public static StoreProductDetailsViewModel FromProduct(ProductPageViewModel product)
    {
        var variants = product.ProductVariants
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

        var selectedVariantId = variants.FirstOrDefault(IsAvailable)?.Id;
        var options = product.SaleOptions.Select(option =>
        {
            var choices = new List<ProductVariantOptionViewModel>();

            foreach (var color in option.ProductSaleOptionColors)
            {
                var colorVariants = variants
                    .Where(x => x.ProductSaleOptionId == option.Id &&
                                x.ProductSaleOptionColorId == color.Id)
                    .ToList();

                if (colorVariants.Count == 0)
                {
                    choices.Add(new ProductVariantOptionViewModel
                    {
                        Label = color.Color,
                        ColorHexCode = NormalizeColor(color.HexCode),
                        IsAvailable = false
                    });
                    continue;
                }

                choices.AddRange(colorVariants.Select(variant =>
                    CreateVariantOption(variant, color.Color, color.HexCode, selectedVariantId)));
            }

            var directVariants = variants
                .Where(x => x.ProductSaleOptionId == option.Id &&
                            x.ProductSaleOptionColorId is null)
                .Select(variant =>
                    CreateVariantOption(variant, option.Title, null, selectedVariantId));

            choices.AddRange(directVariants);

            return new ProductOptionViewModel
            {
                Title = option.Title,
                Choices = choices
            };
        }).ToList();

        var selectedVariant = options
            .SelectMany(x => x.Choices)
            .FirstOrDefault(x => x.IsSelected);

        return new StoreProductDetailsViewModel
        {
            Id = product.Id,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Gallery = product.Images
                .Where(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .Select(x => new ProductGalleryImageViewModel
                {
                    ImageUrl = x.ImageUrl!,
                    AltText = string.IsNullOrWhiteSpace(x.AltText) ? product.Name : x.AltText
                })
                .ToList(),
            Options = options.Where(x => x.Choices.Count > 0).ToList(),
            SelectedVariant = selectedVariant
        };
    }

    private static ProductVariantOptionViewModel CreateVariantOption(
        ProductVariantDetailDto variant,
        string label,
        string? colorHexCode,
        int? selectedVariantId)
    {
        var availableQuantity = Math.Max(0, variant.StockQuantity - variant.ReservedQuantity);

        return new ProductVariantOptionViewModel
        {
            Id = variant.Id,
            Label = label,
            Sku = variant.Sku,
            FinalPrice = variant.FinalPrice,
            AvailableQuantity = availableQuantity,
            IsAvailable = availableQuantity > 0,
            IsSelected = variant.Id == selectedVariantId,
            ColorHexCode = string.IsNullOrWhiteSpace(colorHexCode)
                ? null
                : NormalizeColor(colorHexCode),
            ImageUrl = variant.ProductImages
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                ?.ImageUrl
        };
    }

    private static bool IsAvailable(ProductVariantDetailDto variant) =>
        variant.StockQuantity > variant.ReservedQuantity;

    private static string NormalizeColor(string? colorHexCode) =>
        string.IsNullOrWhiteSpace(colorHexCode) ? "#cbd5e1" : colorHexCode;
}

public class ProductGalleryImageViewModel
{
    public string ImageUrl { get; init; } = string.Empty;
    public string AltText { get; init; } = string.Empty;
}

public class ProductOptionViewModel
{
    public string Title { get; init; } = string.Empty;
    public List<ProductVariantOptionViewModel> Choices { get; init; } = new();
}

public class ProductVariantOptionViewModel
{
    public int? Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public string? ColorHexCode { get; init; }
    public decimal FinalPrice { get; init; }
    public int AvailableQuantity { get; init; }
    public bool IsAvailable { get; init; }
    public bool IsSelected { get; init; }
    public string? ImageUrl { get; init; }

    public string PriceDataValue => FinalPrice.ToString(CultureInfo.InvariantCulture);
}
