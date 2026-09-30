namespace Entities;

public class SiteBanner
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = "";
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string AltText { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
