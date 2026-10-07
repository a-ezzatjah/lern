using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Article
{
    public int Id { get; set; }
    [MaxLength(180)] public string Title { get; set; } = "";
    [MaxLength(160)] public string Slug { get; set; } = "";
    [MaxLength(500)] public string Summary { get; set; } = "";
    public string Content { get; set; } = "";
    [MaxLength(80)] public string Category { get; set; } = "";
    [MaxLength(100)] public string Author { get; set; } = "تحریریه خرازی کوهستانی";
    [MaxLength(400)] public string ImageUrl { get; set; } = "";
    [MaxLength(180)] public string ImageAlt { get; set; } = "";
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
