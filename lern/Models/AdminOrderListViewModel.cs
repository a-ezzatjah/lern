using Entities;
using System.Globalization;

namespace lern.Models;

public sealed class AdminOrderListViewModel
{
    public List<Order> Orders { get; init; } = new();
    public string? Search { get; init; }
    public OrderStatus? Status { get; init; }
    public string? Period { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public static class AdminOrderDisplay
{
    public static TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
    public static DateTime LocalTime(DateTime value) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), TimeZone);
    public static string Date(DateTime value)
    {
        var date = LocalTime(value);
        var calendar = new PersianCalendar();
        return $"{calendar.GetYear(date):0000}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00}";
    }
    public static string Status(OrderStatus status) => status switch
    {
        OrderStatus.PaymentReview => "نیازمند بررسی پرداخت", OrderStatus.Pending => "در انتظار پرداخت", OrderStatus.Paid => "پرداخت شده",
        OrderStatus.Processing => "در حال پردازش", OrderStatus.Shipped => "ارسال شده",
        OrderStatus.Completed => "تحویل داده شده", OrderStatus.Cancelled => "لغو شده", _ => "نامشخص"
    };
    public static string StatusClass(OrderStatus status) => status switch
    {
        OrderStatus.Pending or OrderStatus.PaymentReview => "bg-yellow-100 text-yellow-800",
        OrderStatus.Cancelled => "bg-red-100 text-red-800",
        OrderStatus.Completed => "bg-green-100 text-green-800",
        _ => "bg-blue-100 text-blue-800"
    };
    public static string Payment(PaymentStatus status) => status switch
    {
        PaymentStatus.Successful => "موفق", PaymentStatus.Failed => "ناموفق",
        PaymentStatus.Refunded => "بازگشت وجه", _ => "در انتظار پرداخت"
    };
    public static string Gateway(string? value) => value switch
    {
        "manual" => "دستی", null or "" => "ثبت نشده", _ => value
    };
}
