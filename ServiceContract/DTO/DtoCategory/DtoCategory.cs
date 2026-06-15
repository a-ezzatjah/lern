using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContract.DTO.DtoCategory
{
    public class DtoCategory
    {

        public int ?Id { get; set; }
        public string ?Name { get; set; }
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
        public int? SortOrder { get; set; }
        public ICollection<DtoCategory>? Children { get; set; }
    }
}
