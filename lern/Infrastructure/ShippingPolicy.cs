namespace lern.Infrastructure;

public static class ShippingPolicy
{
    public const string CollectLabel = "پرداخت هزینهٔ ارسال در محل";
    public static string Method(string province) => province.Trim().Replace("استان", "").Trim() == "تهران" ? "courier" : "regional";
    public static string Label(string? method) => method switch { "courier" => "پیک", "freight" => "باربری", "other" => "روش‌های ارسال دیگر", "regional" => "باربری و روش‌های ارسال دیگر (انتخاب توسط مدیر)", _ => "تعیین نشده" };
}
