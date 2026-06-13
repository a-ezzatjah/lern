using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
   public class DtoProductUpdate
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public decimal? Discount { get; set; }
        public bool? HasDiscount { get; set; }
        public DisconTypeEnum? DisconType { get; set; }
        public int? BranchId { get; set; }

    }
}
