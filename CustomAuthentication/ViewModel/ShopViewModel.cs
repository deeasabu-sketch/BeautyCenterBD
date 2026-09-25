using CustomAuthentication.Models;
using System.Collections.Generic;

namespace CustomAuthentication.ViewModel
{
    public class ShopIndexViewModel
    {
        // Products
        public List<Product> Products { get; set; }

        // Categories
        public List<Category> Categories { get; set; }

        // Filters
        public int? SelectedCategoryId { get; set; }
        public string SearchTerm { get; set; }

        // Cart
        public Cart CartViewModel { get; set; }
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }

        // Paging (infinite scroll)
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalProductCount { get; set; }
        public bool HasMoreProducts { get; set; }

        // Set when the product list failed to load, so the view can
        // show a friendly notice instead of just an empty grid.
        public string LoadError { get; set; }

        // Promo code ticker / active promotions
        public List<PromoCode> PromoCodes { get; set; }
    }

    public class CheckoutViewModel
    {
        public Cart Cart { get; set; }

        public string ShippingName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }

        // "Dhaka" or "Outside Dhaka"
        public string DeliveryArea { get; set; }

        public decimal DeliveryCharge { get; set; }

        public int WarehouseId { get; set; }

        public List<System.Web.Mvc.SelectListItem> WarehouseOptions { get; set; }
        public string PromoCode { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PaymentMethod { get; set; }
    }
}