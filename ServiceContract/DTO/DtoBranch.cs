using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DtoBranch
    {
        [Required]
        public string Name { get; set; }
        
        public int Id { get; set; }

    }
}
