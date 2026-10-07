using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Entities;

namespace lern.Models;

public class ArticleListViewModel
{
    public List<Article> Articles { get; set; } = [];
    public List<Article> Latest { get; set; } = [];
    public Dictionary<string, int> Categories { get; set; } = [];
    public string? Query { get; set; }
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
}

public class ArticleDetailsViewModel
{
    public Article Article { get; set; } = null!;
    public ArticleListViewModel Sidebar { get; set; } = new();
}

public class ArticleForm
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "عنوان مقاله را وارد کنید."), StringLength(180)] public string Title { get; set; } = "";
    [Required(ErrorMessage = "نشانی مقاله را وارد کنید."), StringLength(160)]
    [RegularExpression(@"[a-z0-9]+(?:-[a-z0-9]+)*", ErrorMessage = "نشانی را با حروف کوچک انگلیسی، عدد و خط تیره بنویسید.")]
    public string Slug { get; set; } = "";
    [Required(ErrorMessage = "خلاصه مقاله را وارد کنید."), StringLength(500)] public string Summary { get; set; } = "";
    [Required(ErrorMessage = "متن مقاله را وارد کنید."), StringLength(100000)] public string Content { get; set; } = "";
    [Required, StringLength(80)] public string Category { get; set; } = "آموزش خیاطی";
    [Required, StringLength(100)] public string Author { get; set; } = "تحریریه خرازی کوهستانی";
    [Required(ErrorMessage = "توضیح تصویر را وارد کنید."), StringLength(180)] public string ImageAlt { get; set; } = "";
    public IFormFile? Image { get; set; }
    public string? ExistingImageUrl { get; set; }
    public bool IsPublished { get; set; }
}

public static class ArticleDisplay
{
    public static string Date(DateTime? value)
    {
        if (value is null) return "پیش‌نویس";
        var calendar = new PersianCalendar();
        var date = value.Value.AddHours(3.5);
        return $"{calendar.GetYear(date)}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00}";
    }
    public static int ReadingMinutes(string content) => Math.Max(1, (int)Math.Ceiling(content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length / 200d));
}
