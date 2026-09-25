using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CustomAuthentication.ViewModel
{
    public class PurchaseViewModel
    {
        public int PurchaseId { get; set; }

        public string PurchaseNumber { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        public string Status { get; set; }

        public List<PurchaseLineItem> Items { get; set; } = new List<PurchaseLineItem>();

        public IEnumerable<SelectListItem> SupplierOptions { get; set; }
        public IEnumerable<SelectListItem> WarehouseOptions { get; set; }
        public IEnumerable<SelectListItem> ProductOptions { get; set; }
    }

    public class PurchaseLineItem
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitCost { get; set; }
    }
}