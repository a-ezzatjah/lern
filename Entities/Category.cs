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
        public string Slug { get; set; }
        public int? CategoryId { get; set; }
        public Category? category { get; set; }
        public int SortOrder { get; set; }
        public ICollection<Category>? Children { get; set; }
    }
}
