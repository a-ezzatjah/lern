using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContract.DTO.DtoCategory
{
    public class CategoryTreeItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public ICollection<CategoryTreeItemDto>? Children { get; set; }




    }
}
