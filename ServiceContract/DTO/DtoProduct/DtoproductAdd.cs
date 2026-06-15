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
            public decimal? Price { get; set; }
            public string? Description { get; set; }
            public decimal? Discount { get; set; }
            public bool? HasDiscount { get; set; }
            public DisconTypeEnum? DisconType { get; set; }
            
        
    }
}
