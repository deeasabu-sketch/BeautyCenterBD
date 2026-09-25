using CustomAuthentication.ViewModel;
using System.Web;

namespace CustomAuthentication.Helpers
{
    // Session-based cart so anyone browsing the shop (logged in or not)
    // can add products before being asked to log in at checkout.
    public static class CartHelper
    {
        private const string SessionKey = "ShoppingCart";

        public static Cart GetCart(HttpContextBase context)
        {
            var cart = context.Session[SessionKey] as Cart;

            if (cart == null)
            {
                cart = new Cart();
                context.Session[SessionKey] = cart;
            }

            return cart;
        }

        public static void SaveCart(HttpContextBase context, Cart cart)
        {
            context.Session[SessionKey] = cart;
        }

        public static void AddItem(HttpContextBase context, CartItem newItem)
        {
            var cart = GetCart(context);
            var existing = cart.Find(i => i.ProductId == newItem.ProductId);

            if (existing != null)
            {
                existing.Quantity += newItem.Quantity;
            }
            else
            {
                cart.Add(newItem);
            }

            SaveCart(context, cart);
        }

        public static void UpdateQuantity(HttpContextBase context, int productId, int quantity)
        {
            var cart = GetCart(context);
            var item = cart.Find(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            SaveCart(context, cart);
        }

        public static void RemoveItem(HttpContextBase context, int productId)
        {
            var cart = GetCart(context);
            var item = cart.Find(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);
            }

            SaveCart(context, cart);
        }

        public static void Clear(HttpContextBase context)
        {
            context.Session[SessionKey] = new Cart();
        }
    }
}
