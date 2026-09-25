using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class PromoCode
    {
        [Key]
        public int PromoCodeId { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public decimal DiscountPercent { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public int CreatedByUserId { get; set; }
    }
}