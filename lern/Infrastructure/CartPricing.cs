using DTO;
using Entities;

namespace lern.Infrastructure;

public static class CartPricing
{
    public static decimal GetFinalPrice(Product product, ProductVariant variant, DateTime now)
    {
        var variantPrice = ApplyDiscount(variant.Price, variant.DiscountValue, variant.DisconType,
            variant.DiscountStartAt, variant.DiscountEndAt, now);

        return variantPrice < variant.Price
            ? variantPrice
            : ApplyDiscount(variant.Price, product.DiscountValue, product.DiscountType,
                product.DiscountStartAt, product.DiscountEndAt, now);
    }

    private static decimal ApplyDiscount(decimal basePrice, decimal? discountValue,
        DisconTypeEnum? discountType, DateTime? start, DateTime? end, DateTime now)
    {
        if (!discountValue.HasValue || !discountType.HasValue || discountValue <= 0 ||
            (start.HasValue && now < start) || (end.HasValue && now > end))
            return basePrice;

        var finalPrice = discountType switch
        {
            DisconTypeEnum.percent => basePrice - (basePrice * discountValue.Value / 100m),
            DisconTypeEnum.price => basePrice - discountValue.Value,
            _ => basePrice
        };

        return Math.Max(0, finalPrice);
    }
}
