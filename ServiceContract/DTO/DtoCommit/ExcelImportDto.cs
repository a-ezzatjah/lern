using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContract.DTO.DtoCommit
{
    public class ExcelImportDto
    {
       
            public string Name { get; set; }
            public string Slug { get; set; }
            public string Categories { get; set; }
            public string SaleType { get; set; }
            public decimal Price { get; set; }

            public decimal? FixedWeight { get; set; }
            public decimal? FixedLength { get; set; }
            public decimal? FixedWidth { get; set; }
            public decimal? FixedHeight { get; set; }

            public decimal? PerUnitWeight { get; set; }
            public decimal? PerUnitLength { get; set; }
            public decimal? PerUnitWidth { get; set; }
            public decimal? PerUnitHeight { get; set; }
        


    }
}
