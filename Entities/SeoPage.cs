using System.ComponentModel.DataAnnotations;

namespace Entities;

public class SeoPage
{
    [Key, MaxLength(400)] public string Path { get; set; } = "";
    [MaxLength(200)] public string? MetaTitle { get; set; }
    [MaxLength(500)] public string? MetaDescription { get; set; }
    public bool IndexPage { get; set; } = true;
    public bool FollowPage { get; set; } = true;
    public bool IncludeInSitemap { get; set; } = true;
}
