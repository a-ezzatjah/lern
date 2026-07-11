using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoSaleOptionColor;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class DtoProductAdminSaleOption
    {

        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public decimal? BasePrice { get; set; }

        public string? ImageUrl { get; set; }

        public List<DtoProductAdminSaleOptionColor> Colors { get; set; } = new();



    }
}
