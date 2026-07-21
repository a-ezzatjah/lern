

    
public class ProductCardDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    
    // تصویر اصلی محصول (تصویری از ProductImages که IsPrimary آن true است)
    public string ?PrimaryImageUrl { get; set; } 
    
    // ارزان‌ترین قیمت در بین تمام تنوع‌های فعال این محصول
    public decimal? MinPrice { get; set; } 
    public decimal? MaxPrice { get; set; } 

    public decimal? BasePrise { get; set; } 
    
    public bool IsAvailable { get; set; } // مجموع موجودی تمام Variantها > 0


}