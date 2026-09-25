using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    public class DashboardStat
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; } // bootstrap color keyword: primary, success, warning, danger, info
        public string LinkAction { get; set; }
        public string LinkController { get; set; }
    }

    public class DashboardViewModel
    {
        public string RoleName { get; set; }
        public string WarehouseName { get; set; }
        public List<DashboardStat> Stats { get; set; } = new List<DashboardStat>();

        // Which modules this role's dashboard search should cover -
        // drives which sections appear in the search results dropdown.
        public bool SearchProducts { get; set; }
        public bool SearchSales { get; set; }
        public bool SearchPurchases { get; set; }
        public bool SearchStockTransfers { get; set; }
        public bool SearchUsers { get; set; }
        public bool SearchSalaries { get; set; }
    }
}