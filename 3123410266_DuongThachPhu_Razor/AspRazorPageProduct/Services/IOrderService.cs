using AspRazorPageProduct.Models;

namespace AspRazorPageProduct.Services
{
    public interface IOrderService
    {
        void CreateOrder(Order order);
        List<Order> GetAllOrders();
        Order? GetOrderById(string orderId);
        List<Order> GetOrdersForCustomer(string email);
        OrderStatusUpdateResult UpdateStatus(string orderId, string status);
        OrderStatusUpdateResult CancelOrderByCustomer(string orderId, string customerEmail);
        DashboardSummary GetDashboardSummary();
        IReadOnlyList<string> GetValidNextStatuses(string? currentStatus);
    }

    public record OrderStatusUpdateResult(bool Succeeded, string? ErrorMessage = null);
    public record OrderStatusCounts(int PendingConfirmation, int Confirmed, int Shipping, int Delivered, int Cancelled);
    public record DashboardSummary(decimal TotalRevenue, OrderStatusCounts StatusCounts);
}
