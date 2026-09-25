using System;

namespace CustomAuthentication.Models
{
    public class MonthlySales
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string MonthName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalOrders { get; set; }
    }
}