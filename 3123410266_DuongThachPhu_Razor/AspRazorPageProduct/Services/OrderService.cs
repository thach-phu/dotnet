using AspRazorPageProduct.Data;
using AspRazorPageProduct.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AspRazorPageProduct.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Khách tự hủy đơn của chính mình, chỉ khi đơn chưa được Admin duyệt (còn "Chờ xác nhận").</summary>
        public OrderStatusUpdateResult CancelOrderByCustomer(string orderId, string customerEmail)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId && o.CustomerEmail == customerEmail);
            if (order is null) return new(false, "Không tìm thấy đơn hàng.");
            if (order.Status != "Chờ xác nhận")
            {
                return new(false, "Đơn hàng đã được xử lý nên không thể tự hủy. Vui lòng liên hệ cửa hàng để được hỗ trợ.");
            }

            order.Status = "Đã hủy";
            _context.SaveChanges();
            return new(true);
        }

        public void CreateOrder(Order order)
        {
            if (string.IsNullOrEmpty(order.OrderId))
            {
                order.OrderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Guid.NewGuid().ToString("N")[..6];
            }
            order.OrderDate = DateTime.Now;

            // Các CartItem đến từ Session (JSON deserialize) nên Product bên trong
            // chưa được EF theo dõi. Gắn lại Product đã có trong DB để tránh insert trùng.
            foreach (var item in order.Items)
            {
                item.Id = 0;
                item.ProductId = item.Product.Id;
                item.Product = _context.Products.Find(item.ProductId)
                    ?? throw new InvalidOperationException("Sản phẩm trong giỏ hàng không còn tồn tại.");
            }

            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderById(string orderId)
        {
            return _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.OrderId == orderId);
        }

        public List<Order> GetOrdersForCustomer(string email)
        {
            return _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => o.CustomerEmail == email)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public OrderStatusUpdateResult UpdateStatus(string orderId, string status)
        {
            var normalizedStatus = status?.Trim();
            var allowedStatuses = new[] { "Chờ xác nhận", "Đã xác nhận", "Đang giao", "Đã giao", "Đã hủy" };
            if (string.IsNullOrWhiteSpace(normalizedStatus) || !allowedStatuses.Contains(normalizedStatus))
            {
                return new(false, "Trạng thái đơn hàng không hợp lệ.");
            }

            using var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable);
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .SingleOrDefault(o => o.OrderId == orderId);

            if (order is null)
            {
                return new(false, "Không tìm thấy đơn hàng.");
            }

            if (!GetValidNextStatuses(order.Status).Contains(normalizedStatus))
            {
                return new(false, "Chuyển trạng thái không hợp lệ.");
            }

            if (IsDelivered(normalizedStatus))
            {
                if (order.StockDeducted)
                {
                    order.Status = normalizedStatus;
                    _context.SaveChanges();
                    transaction.Commit();
                    return new(true);
                }

                if (order.Items.Count == 0)
                {
                    return new(false, "Đơn hàng không có sản phẩm để xuất kho.");
                }

                var quantities = new Dictionary<int, int>();
                foreach (var item in order.Items)
                {
                    if (item.ProductId <= 0 || item.Product is null)
                    {
                        return new(false, "Đơn hàng có sản phẩm không hợp lệ.");
                    }

                    if (item.Quantity <= 0)
                    {
                        return new(false, $"Số lượng sản phẩm “{item.Product.Name}” không hợp lệ.");
                    }

                    quantities[item.ProductId] = quantities.GetValueOrDefault(item.ProductId) + item.Quantity;
                }

                foreach (var quantity in quantities)
                {
                    var product = _context.Products.SingleOrDefault(p => p.Id == quantity.Key);
                    if (product is null)
                    {
                        return new(false, "Một sản phẩm trong đơn hàng không còn tồn tại.");
                    }

                    if (product.Stock < quantity.Value)
                    {
                        return new(false, $"Sản phẩm “{product.Name}” chỉ còn {product.Stock} trong kho, không đủ số lượng cần giao.");
                    }
                }

                foreach (var quantity in quantities)
                {
                    var product = _context.Products.Single(p => p.Id == quantity.Key);
                    product.Stock -= quantity.Value;
                }

                order.StockDeducted = true;
            }

            order.Status = normalizedStatus;
            _context.SaveChanges();
            transaction.Commit();
            return new(true);
        }

        private static bool IsDelivered(string? status) =>
            status is "Đã giao" or "Completed" or "Hoàn thành";

        public IReadOnlyList<string> GetValidNextStatuses(string? currentStatus) => Normalize(currentStatus) switch
        {
            "Chờ xác nhận" => ["Đã xác nhận", "Đã hủy"],
            "Đã xác nhận" => ["Đang giao", "Đã hủy"],
            "Đang giao" => ["Đã giao", "Đã hủy"],
            _ => []
        };

        private static string Normalize(string? status) => status switch
        {
            "Pending" => "Chờ xác nhận",
            "Completed" or "Hoàn thành" => "Đã giao",
            _ => status ?? ""
        };

        public DashboardSummary GetDashboardSummary()
        {
            var orders = _context.Orders.AsNoTracking().Select(order => new { order.Status, order.TotalAmount }).ToList();

            var totalRevenue = orders
                .Where(order => order.Status != "Đã hủy")
                .Sum(order => order.TotalAmount);

            var statusCounts = new OrderStatusCounts(
                PendingConfirmation: orders.Count(order => order.Status is "Chờ xác nhận" or "Pending"),
                Confirmed: orders.Count(order => order.Status == "Đã xác nhận"),
                Shipping: orders.Count(order => order.Status == "Đang giao"),
                Delivered: orders.Count(order => IsDelivered(order.Status)),
                Cancelled: orders.Count(order => order.Status == "Đã hủy"));

            return new DashboardSummary(totalRevenue, statusCounts);
        }
    }
}
