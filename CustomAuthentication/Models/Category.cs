using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Category
    {
        
        
            [Key]
            public int CategoryId { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(300)]
            public string Description { get; set; }

            public bool IsActive { get; set; } = true;

            public virtual ICollection<Product> Products { get; set; }
        
    }
}