using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CustomAuthentication.ViewModel
{
    public class StockTransferCreateViewModel
    {
        [Required]
        public int FromWarehouseId { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }

        public List<StockTransferLineItem> Items { get; set; } = new List<StockTransferLineItem>();

        public IEnumerable<SelectListItem> WarehouseOptions { get; set; }
        public IEnumerable<SelectListItem> ProductOptions { get; set; }
    }

    public class StockTransferLineItem
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}