using Entities;

namespace lern.Models;

public sealed class AdminSettingsViewModel
{
    public List<SiteBanner> Banners { get; set; } = [];
    public List<Category> MainCategories { get; set; } = [];
    public List<SiteInboxItem> Inbox { get; set; } = [];
}
