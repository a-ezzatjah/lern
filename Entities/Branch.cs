using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Branch
    {



        public string Name { get; set; }
        [Key]
        public int Id { get; set; }


       public ICollection<Product> products { get; set; }


    }
}
