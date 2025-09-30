using XTinyLog.Samples.Web.Models;

namespace XTinyLog.Samples.Web.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<ProcessOrderResult> ProcessOrderAsync(ProcessOrderRequest request);
}