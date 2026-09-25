using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomAuthentication.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderStatus { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public bool IsActive { get; set; }

        // Invoice Information
        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        [StringLength(30)]
        public string CustomerPhone { get; set; }

        [StringLength(150)]
        public string CustomerEmail { get; set; }

        [StringLength(500)]
        public string CustomerAddress { get; set; }
        [StringLength(30)]
        public string PromoCodeUsed { get; set; }
        public string PromoCode { get; set; }
        public string Status { get; set; }

        //=========================//
        //====Bkash ApI============//
        //========================//
        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string TransactionId { get; set; }

        public string PaymentID { get; set; }



        // Order Items
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

       

        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        

        // Which warehouse/store fulfills this order.
        [Required]
        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [Required, StringLength(150)]
        public string ShippingName { get; set; }

        [Required, StringLength(30)]
        public string ShippingPhone { get; set; }

        [Required, StringLength(300)]
        public string ShippingAddress { get; set; }

        // "Dhaka" or "Outside Dhaka" - decides DeliveryCharge (60 / 120 taka).
        [Required, StringLength(30)]
        public string DeliveryArea { get; set; }

        [Required]
        public decimal DeliveryCharge { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; }

        

        
        

        // Links to the Sale created when this order is Confirmed
        // (stock officially leaves the warehouse at that point).
        public int? SaleId { get; set; }
        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }


    }

    public static class DeliveryAreas
    {
        public const string Dhaka = "Dhaka";
        public const string OutsideDhaka = "Outside Dhaka";

        public const decimal DhakaCharge = 60m;
        public const decimal OutsideDhakaCharge = 120m;

        public static decimal GetCharge(string area)
        {
            if (string.IsNullOrEmpty(area))
                return 0m;

            if (area.Equals(Dhaka, StringComparison.OrdinalIgnoreCase))
                return DhakaCharge;

            if (area.Equals(OutsideDhaka, StringComparison.OrdinalIgnoreCase))
                return OutsideDhakaCharge;

            return OutsideDhakaCharge;
        }
    }

}