using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CustomAuthentication.ViewModel
{
    public class SaleViewModel
    {
        public int SaleId { get; set; }

        public string SaleNumber { get; set; }

        public int? CustomerId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        public string Status { get; set; }
       

        public List<SaleLineItem> Items { get; set; } = new List<SaleLineItem>();

        public IEnumerable<SelectListItem> CustomerOptions { get; set; }
        public IEnumerable<SelectListItem> WarehouseOptions { get; set; }
        public IEnumerable<SelectListItem> ProductOptions { get; set; }
    }

    public class SaleLineItem
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }
    }
}