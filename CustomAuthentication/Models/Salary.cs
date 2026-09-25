using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Salary
    {
        [Key]
        public int SalaryId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        public decimal BasicSalary { get; set; }

        public decimal Bonus { get; set; }

        public decimal Deduction { get; set; }

        [Required]
        public decimal NetSalary { get; set; }

        // Which month/year this salary record is for.
        [Required]
        public int SalaryMonth { get; set; } // 1-12

        [Required]
        public int SalaryYear { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid

        public DateTime? PaidDate { get; set; }

        // HR user who recorded/processed this salary entry.
        public int CreatedByUserId { get; set; }

        [StringLength(300)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; } = true;
    }
}