using System.ComponentModel.DataAnnotations.Schema;

namespace AspRazorPageProduct.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; } = 1;

        [NotMapped]
        public decimal TotalPrice => Product.Price * Quantity;
    }
}
