using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    public class PromoCodeViewModel
    {
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
    }
}