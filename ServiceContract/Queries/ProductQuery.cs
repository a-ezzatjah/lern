using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.Enums;

namespace ServiceContract.Quaries
{
    public class ProductQuery
    {

        public string? SearchText { get; set; }


        [EnumDataType(typeof(EnumProductSearchType))]
        public EnumProductSearchType SearchType { get; set; } = EnumProductSearchType.Name;

        public bool? HasDiscount { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }




        [EnumDataType(typeof(EnumProductSortType))]
        public EnumProductSortType SortType { get; set; } = EnumProductSortType.Id;

        [EnumDataType(typeof(OrderEnum))]
        public OrderEnum Order { get; set; } = OrderEnum.ASC;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;


    }
}
