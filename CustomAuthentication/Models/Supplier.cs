using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}