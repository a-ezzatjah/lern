using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace ServiceContract.DTO.DtoProduct
{
    public class DtoproductAdd
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }
    }
}
