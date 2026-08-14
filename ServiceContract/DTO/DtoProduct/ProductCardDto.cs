namespace ServiceContract.DTO.DtoProduct;

public class ProductCardDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    // تصویر اصلی محصول
    public string? PrimaryImageUrl { get; set; }

    // برای محصولات چندحالته:
    // ارزان‌ترین قیمت نهایی بین Variantهای فعال
    public decimal? MinPrice { get; set; }

    // برای محصولات تکی:
    // قیمت نهایی همان Variant بعد از تخفیف
    public decimal? FinalPrice { get; set; }

    // آیا محصول چند حالت فروش یا چند Variant دارد؟
    public bool HasMultipleOptions { get; set; }

    // آیا محصول یا یکی از Variantها تخفیف فعال دارد؟
    public bool HasDiscount { get; set; }

    // درصد تخفیف برای نمایش روی کارت
    // برای محصول تکی: درصد همان Variant/Product
    // برای محصول چندحالته: بیشترین درصد تخفیف فعال
    public decimal? DiscountPercent { get; set; }

    // مجموع موجودی Variantهای فعال > 0
    public bool IsAvailable { get; set; }

}
