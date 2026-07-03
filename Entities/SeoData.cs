using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class SeoData
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? Keywords { get; set; }

        [MaxLength(500)]
        public string? CanonicalUrl { get; set; }

        public string? Schema { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}