using MVC.Data;
using MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public sealed record OrderStatusCounts(int PendingConfirmation, int Confirmed, int Shipping, int Delivered, int Cancelled);
public sealed record DashboardSummary(decimal TotalRevenue, OrderStatusCounts StatusCounts);

public sealed class OrderService
{
    private readonly StoreDbContext db;

    public OrderService(StoreDbContext db)
    {
        this.db = db;
    }

    public List<Order> GetOrders()
    {
        return db.Orders
            .Include(order => order.Items)
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToList();
    }

    public List<Order> GetOrdersForUser(int userId)
    {
        return db.Orders
            .Where(order => order.UserId == userId)
            .Include(order => order.Items)
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToList();
    }

    public Order? GetOrderForUser(int orderId, int userId)
    {
        return db.Orders
            .Where(order => order.Id == orderId && order.UserId == userId)
            .Include(order => order.Items)
            .AsNoTracking()
            .FirstOrDefault();
    }

    public Order? GetOrderByCode(string orderCode)
    {
        return db.Orders
            .Include(order => order.Items)
            .AsNoTracking()
            .FirstOrDefault(order => order.OrderCode == orderCode);
    }

    public Order CreateOrder(
        string customerName,
        string customerPhone,
        string customerAddress,
        string? note,
        IReadOnlyCollection<(Product Product, int Quantity)> items,
        decimal shippingFee,
        decimal discount = 0,
        int? userId = null)
    {
        var orderCode = $"PHU{Random.Shared.Next(100000, 999999)}";
        var subTotal = items.Sum(item => item.Product.Price * item.Quantity);
        var grandTotal = Math.Max(0, subTotal + shippingFee - discount);

        var order = new Order
        {
            OrderCode = orderCode,
            UserId = userId,
            CustomerName = customerName.Trim(),
            CustomerPhone = customerPhone.Trim(),
            CustomerAddress = customerAddress.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            SubTotal = subTotal,
            ShippingFee = shippingFee,
            GrandTotal = grandTotal,
            Status = "Chờ xác nhận",
            CreatedAt = DateTime.Now,
            Items = items.Select(item => new OrderItem
            {
                ProductId = item.Product.Id,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
                Quantity = item.Quantity,
                ImageUrl = item.Product.ImageUrl
            }).ToList()
        };

        db.Orders.Add(order);
        db.SaveChanges();
        return order;
    }

    public (Order? Order, string? Error) TryCreateOrder(
        string customerName,
        string customerPhone,
        string customerAddress,
        string? note,
        IReadOnlyCollection<(Product Product, int Quantity)> items,
        decimal shippingFee,
        decimal discount = 0,
        int? userId = null)
    {
        var requestedQuantities = items
            .GroupBy(item => item.Product.Id)
            .ToDictionary(group => group.Key, group => group.Sum(item => (long)item.Quantity));

        if (requestedQuantities.Any(item => item.Value <= 0))
            return (null, "Số lượng sản phẩm trong giỏ hàng không hợp lệ.");

        using var transaction = db.Database.BeginTransaction();
        var products = db.Products
            .Where(product => requestedQuantities.Keys.Contains(product.Id))
            .ToDictionary(product => product.Id);

        var stockErrors = requestedQuantities
            .Where(request => !products.TryGetValue(request.Key, out var product)
                || request.Value > product.Stock)
            .Select(request =>
            {
                var product = products.GetValueOrDefault(request.Key);
                var name = product?.Name ?? $"sản phẩm #{request.Key}";
                var available = product?.Stock ?? 0;
                return $"{name}: cần {request.Value}, chỉ còn {available}.";
            })
            .ToList();

        if (stockErrors.Count > 0)
        {
            transaction.Rollback();
            return (null, $"Không đủ tồn kho: {string.Join(" ", stockErrors)}");
        }

        var order = CreateOrder(
            customerName, customerPhone, customerAddress, note, items,
            shippingFee, discount, userId);
        transaction.Commit();
        return (order, null);
    }

    public IReadOnlyList<string> GetValidNextStatuses(string currentStatus) => currentStatus switch
    {
        "Chờ xác nhận" => ["Đã xác nhận", "Đã hủy"],
        "Đã xác nhận" => ["Đang giao", "Đã hủy"],
        "Đang giao" => ["Đã giao", "Đã hủy"],
        _ => []
    };

    public (bool Success, string? Error) UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == orderId);
        if (order is null) return (false, "Không tìm thấy đơn hàng.");
        if (!GetValidNextStatuses(order.Status).Contains(newStatus))
            return (false, "Chuyển trạng thái không hợp lệ.");
        var shouldDecrementInventory = string.Equals(newStatus, "Đã giao", StringComparison.Ordinal);
        using var transaction = shouldDecrementInventory ? db.Database.BeginTransaction() : null;
        if (shouldDecrementInventory)
        {
            if (!order.InventoryDecremented)
            {
                foreach (var item in order.Items.GroupBy(item => item.ProductId))
                {
                    var quantity = item.Sum(line => line.Quantity);
                    var changed = db.Products
                        .Where(product => product.Id == item.Key && product.Stock >= quantity)
                        .ExecuteUpdate(setters => setters.SetProperty(product => product.Stock, product => product.Stock - quantity));
                    if (changed != 1)
                        return (false, $"Không đủ tồn kho cho sản phẩm trong đơn hàng ({item.Key}).");

                }
                order.InventoryDecremented = true;
            }
        }
        order.Status = newStatus;
        db.SaveChanges();
        transaction?.Commit();
        return (true, null);
    }

    /// <summary>Khách tự hủy đơn của chính mình, chỉ khi đơn chưa được Admin duyệt (còn "Chờ xác nhận").</summary>
    public (bool Success, string? Error) CancelOrderByCustomer(int orderId, int userId)
    {
        var order = db.Orders.FirstOrDefault(current => current.Id == orderId && current.UserId == userId);
        if (order is null) return (false, "Không tìm thấy đơn hàng.");
        if (order.Status != "Chờ xác nhận")
        {
            return (false, "Đơn hàng đã được xử lý nên không thể tự hủy. Vui lòng liên hệ cửa hàng để được hỗ trợ.");
        }

        order.Status = "Đã hủy";
        db.SaveChanges();
        return (true, null);
    }

    public DashboardSummary GetDashboardSummary()
    {
        var orders = db.Orders.AsNoTracking().Select(order => new { order.Status, order.GrandTotal }).ToList();

        var totalRevenue = orders
            .Where(order => order.Status != "Đã hủy")
            .Sum(order => order.GrandTotal);

        var statusCounts = new OrderStatusCounts(
            PendingConfirmation: orders.Count(order => order.Status == "Chờ xác nhận"),
            Confirmed: orders.Count(order => order.Status == "Đã xác nhận"),
            Shipping: orders.Count(order => order.Status == "Đang giao"),
            Delivered: orders.Count(order => order.Status == "Đã giao"),
            Cancelled: orders.Count(order => order.Status == "Đã hủy"));

        return new DashboardSummary(totalRevenue, statusCounts);
    }

}
