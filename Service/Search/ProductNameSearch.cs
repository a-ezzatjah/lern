using System.Linq.Expressions;
using Entities;

namespace Service.Search;

public static class ProductNameSearch
{
    // Keep normalization inside the expression so EF translates the entire filter to SQL.
    private static readonly Expression<Func<Product, string>> NormalizedName = product => product.Name
        .Replace("ي", "ی").Replace("ك", "ک").Replace("\u200c", " ")
        .Replace("۰", "0").Replace("۱", "1").Replace("۲", "2").Replace("۳", "3").Replace("۴", "4")
        .Replace("۵", "5").Replace("۶", "6").Replace("۷", "7").Replace("۸", "8").Replace("۹", "9")
        .Replace("٠", "0").Replace("١", "1").Replace("٢", "2").Replace("٣", "3").Replace("٤", "4")
        .Replace("٥", "5").Replace("٦", "6").Replace("٧", "7").Replace("٨", "8").Replace("٩", "9");

    public static IQueryable<Product> Apply(IQueryable<Product> products, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return products;

        var normalized = search.Replace('ي', 'ی').Replace('ك', 'ک').Replace('\u200c', ' ');
        for (var digit = 0; digit <= 9; digit++)
            normalized = normalized.Replace((char)('۰' + digit), (char)('0' + digit))
                .Replace((char)('٠' + digit), (char)('0' + digit));

        var terms = normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
        // Sizes should not broaden a named-product search to unrelated numeric matches.
        // Keep all textual terms required, while allowing different sizes of the same product.
        var words = terms.Where(term => !term.All(char.IsDigit)).ToArray();
        var requiredTerms = words.Length > 0 ? words : terms;
        Expression? matches = null;
        foreach (var term in requiredTerms)
        {
            var contains = Expression.Call(NormalizedName.Body, nameof(string.Contains), Type.EmptyTypes,
                Expression.Constant(term));
            matches = matches is null ? contains : Expression.AndAlso(matches, contains);
        }

        return matches is null ? products : products.Where(
            Expression.Lambda<Func<Product, bool>>(matches, NormalizedName.Parameters));
    }
}
