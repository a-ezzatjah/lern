namespace lern.Models;

public sealed class AdminSalesReportViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime ReportedTo { get; set; }
    public bool IsTruncated { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int? CategoryId { get; set; }
    public List<ReportCategoryOption> Categories { get; set; } = [];
    public decimal Sales { get; set; }
    public int OrderCount { get; set; }
    public int NewCustomers { get; set; }
    public decimal AverageOrder => OrderCount == 0 ? 0 : Sales / OrderCount;
    public List<ReportPoint> Daily { get; set; } = [];
    public string[] MonthLabels { get; set; } = [];
    public decimal[] MonthSales { get; set; } = [];
    public string[] CategoryLabels { get; set; } = [];
    public decimal[] CategorySales { get; set; } = [];
}

public sealed record ReportCategoryOption(int Id, string Name);
public sealed record ReportPoint(DateTime Date, int Orders, decimal Sales, int NewCustomers);
