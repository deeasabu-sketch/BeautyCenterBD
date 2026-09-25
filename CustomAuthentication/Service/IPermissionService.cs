using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAuthentication.Service
{
    public interface IPermissionService
    {
        bool CanView(string roleName, string moduleName);
        bool CanCreate(string roleName, string moduleName);
        bool CanEdit(string roleName, string moduleName);
        bool CanDelete(string roleName, string moduleName);
    }
}
