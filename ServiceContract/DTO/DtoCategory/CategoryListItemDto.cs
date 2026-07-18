using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContract.DTO.DtoCategory
{
    public class CategoryListItemDto
    {


        public int  Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int? ParentId { get; set; }

        public string? ParentName { get; set; }
        public int? SortOrder { get; set; }

        public int ChildrenCount { get; set; }



    }
}
