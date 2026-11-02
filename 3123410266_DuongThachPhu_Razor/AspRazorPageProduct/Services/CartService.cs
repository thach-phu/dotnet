using AspRazorPageProduct.Helpers;
using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public class CartService : ICartService
    {
        private const string CART_KEY = "Cart_SessionKey";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductService productService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetCart()
        {
            var cart = Session.GetObjectFromJson<List<CartItem>>(CART_KEY);
            return cart ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            Session.SetObjectAsJson(CART_KEY, cart);
        }

        public void AddToCart(int productId, int quantity = 1)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                var product = _productService.GetById(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Product = product,
                        Quantity = quantity
                    });
                }
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            Session.Remove(CART_KEY);
        }

        public int GetTotalCount()
        {
            return GetCart().Sum(item => item.Quantity);
        }

        public decimal GetGrandTotal()
        {
            return GetCart().Sum(item => item.TotalPrice);
        }
    }
}
