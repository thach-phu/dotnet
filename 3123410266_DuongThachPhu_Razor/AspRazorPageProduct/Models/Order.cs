namespace AspRazorPageProduct.Models
{
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "COD"; // COD, Banking, MoMo
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Chờ xác nhận"; // Chờ xác nhận, Đã xác nhận, Đang giao, Đã giao, Đã hủy
        public bool StockDeducted { get; set; }
    }
}
