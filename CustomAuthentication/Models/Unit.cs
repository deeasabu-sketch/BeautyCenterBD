using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [Required, StringLength(10)]
        public string ShortCode { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; }
    }
}