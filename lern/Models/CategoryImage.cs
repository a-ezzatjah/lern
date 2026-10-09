using Entities;

namespace lern.Models;

public static class CategoryImage
{
    public static string GetUrl(Category category)
    {
        if (!string.IsNullOrWhiteSpace(category.ImageUrl)) return category.ImageUrl;
        var name = category.Slug switch
        {
            "buttons-and-hardware" => "hardware",
            "catalog-0001" => "thread",
            "catalog-0040" => "needles",
            "catalog-0071" => "zippers",
            "catalog-0108" => "scissors",
            "catalog-0127" => "sewing-tools",
            "catalog-0164" => "machine-tools",
            "catalog-0186" => "beads",
            "catalog-extra-000" or "elastic" or "elastic-qizan" or "elastic-iranian" or "elastic-imported" => "elastic",
            "catalog-extra-020" => "curtain",
            "catalog-extra-026" => "yarn",
            "catalog-extra-046" => "ribbon",
            "catalog-extra-086" => "cord",
            "catalog-extra-097" => "snap-press",
            "catalog-extra-123" => "buttons",
            "catalog-extra-147" => "coat-buttons",
            _ => "sewing-tools"
        };
        return $"/assets/images/category/haberdashery/{name}-photo.png";
    }
}
