using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface ICartService
    {
        List<CartItem> GetCart();
        void AddToCart(int productId, int quantity = 1);
        void UpdateQuantity(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        int GetTotalCount();
        decimal GetGrandTotal();
    }
}
