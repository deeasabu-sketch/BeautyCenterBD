using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomAuthentication.ViewModel
{
    [Serializable]
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImagePath { get; set; }
        public decimal UnitPrice { get; set; } // price AFTER discount, at time of adding
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }

    [Serializable]
    public class Cart : List<CartItem>
    {
        public decimal Total => this.Sum(i => i.LineTotal);
        public int TotalItems => this.Sum(i => i.Quantity);
    }
}