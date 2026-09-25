using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required, StringLength(50)]
        public string RoleName { get; set; }

        [Required]
        public int ModuleId { get; set; }
        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }

        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}