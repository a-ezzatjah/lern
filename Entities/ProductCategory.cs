using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ProductCategory
    {

        public int ParentId { get; set; }

        public Category Parent { set; get; } = null!;


        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;


    }
}
