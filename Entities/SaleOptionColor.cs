using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SaleOptionColor
    {

        int Id { get; set; }

        string Color { get; set; } = null!;

        int SaleOptionId { get; set; }

        SearchOption saleOption { get; set; }

        decimal Price { get; set; }


        //public decimal CalculateBasePrice(decimal quantity) => quantity * Price;

        //public decimal CalculateBaseWeight(decimal quantity)
        //{
        //    if (PerUnitWeight.HasValue)
        //        return quantity * PerUnitWeight.Value;

        //    if (FixedWeight.HasValue)
        //        return quantity * FixedWeight.Value;

        //    return 0;
        //}

    }
}
