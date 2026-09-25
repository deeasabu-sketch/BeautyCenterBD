using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomAuthentication.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(150)]
        public string Description { get; set; }

        // Display order in the Admin/HR permission-management screen.
        public int SortOrder { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}