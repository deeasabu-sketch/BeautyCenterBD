using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;



namespace CustomAuthentication.Models
    {
        public class Brand
        {
            [Key]
            public int BrandId { get; set; }

            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(300)]
            public string Description { get; set; }

            // Logo/banner image shown in the shop's brand carousel.
            [StringLength(300)]
            public string ImagePath { get; set; }

            public bool IsActive { get; set; } = true;

            public virtual ICollection<Product> Products { get; set; }
        }
    }


