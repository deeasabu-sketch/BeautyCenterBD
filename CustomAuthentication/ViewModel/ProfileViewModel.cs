using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    public class ProfileViewModel
    {
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Display(Name = "Account Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Profile Picture")]
        public string ImagePath { get; set; }
    }
}