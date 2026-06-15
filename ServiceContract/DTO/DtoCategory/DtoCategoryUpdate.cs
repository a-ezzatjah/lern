using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContract.DTO.DtoCategory
{
    public class DtoCategoryUpdate
    {


        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public int? ParentId { get; set; }
        public int? SortOrder { get; set; }




    }
}
