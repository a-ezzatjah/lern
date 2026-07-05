using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Category
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public int SortOrder { get; set; }
        public SeoData? Seo { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
