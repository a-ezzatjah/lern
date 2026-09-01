namespace lern.Models;

public class AdminDashboardViewModel
{
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int CategoryCount { get; set; }
    public int AvailableStock { get; set; }
    public string[] ChartLabels { get; set; } = Array.Empty<string>();
    public decimal[] ChartValues { get; set; } = Array.Empty<decimal>();
    public int OrderCount { get; set; }
    public decimal SalesTotal { get; set; }
}
