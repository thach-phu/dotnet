using BlazorWeb.Data;
using BlazorWeb.Models;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlazorWeb.Services;

public sealed class OrderService
{
    public sealed record StatusUpdateResult(bool Success, string? Error = null);
    public sealed record CreateOrderResult(bool Success, Order? Order = null, string? Error = null);
    public sealed record OrderStatusCounts(int PendingConfirmation, int Confirmed, int Shipping, int Delivered, int Cancelled);
    public sealed record DashboardSummary(decimal TotalRevenue, OrderStatusCounts StatusCounts);

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
        var result = TryCreateOrder(customerName, customerPhone, customerAddress, note, items, shippingFee, discount, userId);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error);
        }

        return result.Order!;
    }

    public CreateOrderResult TryCreateOrder(
        string customerName,
        string customerPhone,
        string customerAddress,
        string? note,
        IReadOnlyCollection<(Product Product, int Quantity)> items,
        decimal shippingFee,
        decimal discount = 0,
        int? userId = null)
    {
        var requiredByProduct = items
            .GroupBy(item => item.Product.Id)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));
        if (requiredByProduct.Values.Any(quantity => quantity <= 0))
        {
            return new(false, Error: "Số lượng sản phẩm trong giỏ hàng không hợp lệ.");
        }

        IDbContextTransaction? transaction = db.Database.IsRelational()
            ? db.Database.BeginTransaction(IsolationLevel.Serializable)
            : null;
        try
        {
            var products = db.Products
                .Where(product => requiredByProduct.Keys.Contains(product.Id))
                .ToDictionary(product => product.Id);
            var missingProduct = requiredByProduct.Keys.FirstOrDefault(productId => !products.ContainsKey(productId));
            if (missingProduct != 0)
            {
                transaction?.Rollback();
                return new(false, Error: $"Sản phẩm mã {missingProduct} không còn tồn tại.");
            }

            var insufficient = requiredByProduct
                .FirstOrDefault(entry => products[entry.Key].StockQuantity < entry.Value);
            if (insufficient.Key != 0)
            {
                var product = products[insufficient.Key];
                transaction?.Rollback();
                return new(false,
                    Error: $"Không đủ tồn kho cho {product.Name}. Còn {product.StockQuantity}, cần {insufficient.Value}.");
            }

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
            transaction?.Commit();
            return new(true, order);
        }
        catch
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public IReadOnlyList<string> GetValidNextStatuses(string currentStatus) => currentStatus switch
    {
        "Chờ xác nhận" => ["Đã xác nhận", "Đã hủy"],
        "Đã xác nhận" => ["Đang giao", "Đã hủy"],
        "Đang giao" => ["Đã giao", "Đã hủy"],
        _ => []
    };

    public StatusUpdateResult UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = db.Orders
            .Include(current => current.Items)
            .FirstOrDefault(current => current.Id == orderId);
        if (order is null) return new(false, "Không tìm thấy đơn hàng.");
        if (!GetValidNextStatuses(order.Status).Contains(newStatus))
        {
            return new(false, "Chuyển trạng thái không hợp lệ.");
        }

        if (newStatus != "Đã giao")
        {
            order.Status = newStatus;
            db.SaveChanges();
            return new(true);
        }

        var requiredByProduct = order.Items
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));
        var products = db.Products
            .Where(product => requiredByProduct.Keys.Contains(product.Id))
            .ToDictionary(product => product.Id);

        var missingProduct = requiredByProduct.Keys.FirstOrDefault(productId => !products.ContainsKey(productId));
        if (missingProduct != 0)
        {
            return new(false, $"Sản phẩm mã {missingProduct} trong đơn không còn tồn tại.");
        }

        var insufficient = requiredByProduct
            .FirstOrDefault(entry => products[entry.Key].StockQuantity < entry.Value);
        if (insufficient.Key != 0)
        {
            var product = products[insufficient.Key];
            return new(false,
                $"Không đủ tồn kho cho {product.Name}. Còn {product.StockQuantity}, cần {insufficient.Value}.");
        }

        IDbContextTransaction? transaction = db.Database.IsRelational()
            ? db.Database.BeginTransaction()
            : null;
        try
        {
            if (db.Database.IsRelational())
            {
                // Claim delivery and decrement stock with guarded SQL updates. This
                // prevents two admins from fulfilling the same order concurrently.
                var claimed = db.Orders
                    .Where(current => current.Id == orderId && current.Status != "Đã giao")
                    .ExecuteUpdate(setters => setters.SetProperty(current => current.Status, "Đã giao"));
                if (claimed == 0)
                {
                    transaction?.Rollback();
                    return new(false, "Đơn hàng đã giao nên trạng thái không thể thay đổi.");
                }

                foreach (var (productId, quantity) in requiredByProduct)
                {
                    var updated = db.Products
                        .Where(product => product.Id == productId && product.StockQuantity >= quantity)
                        .ExecuteUpdate(setters => setters.SetProperty(
                            product => product.StockQuantity,
                            product => product.StockQuantity - quantity));
                    if (updated != 1)
                    {
                        transaction?.Rollback();
                        return new(false, $"Không đủ tồn kho cho {products[productId].Name}.");
                    }
                }
            }
            else
            {
                foreach (var (productId, quantity) in requiredByProduct)
                {
                    products[productId].StockQuantity -= quantity;
                }

                order.Status = newStatus;
                db.SaveChanges();
            }

            if (db.Database.IsRelational())
            {
                transaction?.Commit();
            }
            return new(true);
        }
        catch
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    /// <summary>Khách tự hủy đơn của chính mình, chỉ khi đơn chưa được Admin duyệt (còn "Chờ xác nhận").</summary>
    public StatusUpdateResult CancelOrderByCustomer(int orderId, int userId)
    {
        var order = db.Orders.FirstOrDefault(current => current.Id == orderId && current.UserId == userId);
        if (order is null) return new(false, "Không tìm thấy đơn hàng.");
        if (order.Status != "Chờ xác nhận")
        {
            return new(false, "Đơn hàng đã được xử lý nên không thể tự hủy. Vui lòng liên hệ cửa hàng để được hỗ trợ.");
        }

        order.Status = "Đã hủy";
        db.SaveChanges();
        return new(true);
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
