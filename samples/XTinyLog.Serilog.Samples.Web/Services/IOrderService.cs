using XTinyLog.Serilog.Samples.Web.Models;

namespace XTinyLog.Serilog.Samples.Web.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrdersAsync(int? userId = null);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task<bool> CancelOrderAsync(int id, string reason);
}