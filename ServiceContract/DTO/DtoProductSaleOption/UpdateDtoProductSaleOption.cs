using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace ServiceContract.DTO.UpdateDtoProductSaleOption
{
    public class UpdateDtoProductSaleOption
    {

        public int Id { get; set; }
        public string? Name { get; set; } = null!;
        public decimal? BasePrice { get; set; }
        public string? InputLabel { get; set; }
        public string? UnitName { get; set; }


        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal Step { get; set; } = 1;

        public decimal? FixedWeight { get; set; }
        public decimal? FixedLength { get; set; }
        public decimal? FixedWidth { get; set; }
        public decimal? FixedHeight { get; set; }

        public decimal? PerUnitWeight { get; set; }
        public decimal? PerUnitLength { get; set; }
        public decimal? PerUnitWidth { get; set; }
        public decimal? PerUnitHeight { get; set; }


        public ICollection<SaleOptionColor> SaleOptionColors { get; set; } = new List<SaleOptionColor>();



    }
}
