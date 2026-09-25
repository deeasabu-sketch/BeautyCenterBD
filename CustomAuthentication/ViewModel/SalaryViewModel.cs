using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CustomAuthentication.ViewModel
{
    public class SalaryViewModel
    {
        public int SalaryId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public decimal BasicSalary { get; set; }

        public decimal Bonus { get; set; }

        public decimal Deduction { get; set; }

        [Required]
        public int SalaryMonth { get; set; }

        [Required]
        public int SalaryYear { get; set; }

        [StringLength(300)]
        public string Remarks { get; set; }

        public IEnumerable<SelectListItem> UserOptions { get; set; }
        public IEnumerable<SelectListItem> MonthOptions { get; set; }
    }
}