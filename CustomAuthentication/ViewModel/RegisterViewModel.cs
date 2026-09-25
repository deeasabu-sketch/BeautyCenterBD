using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    public class RegisterViewModel
    {
        [Required,Display(Name ="Full Name")]
        public string FullName { get; set; }
        [Required, StringLength(30), Display(Name = "Phone")]
        public string Phone { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required,DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [ DataType(DataType.Password),Compare("Password")]
        public string ConfirmPassword { get; set; }
     
    }
}