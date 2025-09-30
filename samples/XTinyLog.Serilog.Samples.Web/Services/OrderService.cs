using XTinyLog.Serilog.Samples.Web.Models;

namespace XTinyLog.Serilog.Samples.Web.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private static readonly List<Order> _orders = new();
    private static int _nextId = 1;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync(int? userId = null)
    {
        _logger.LogDebug("Retrieving orders with user filter {UserId}", userId);
        
        // Simulate async database operation
        await Task.Delay(75);
        
        var filteredOrders = userId.HasValue 
            ? _orders.Where(o => o.UserId == userId.Value).ToList()
            : _orders.ToList();
        
        _logger.LogDebug("Found {OrderCount} orders for user filter {UserId}", filteredOrders.Count, userId);
        
        return filteredOrders.OrderByDescending(o => o.CreatedAt);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving order {OrderId} from data store", id);
        
        // Simulate async database operation
        await Task.Delay(50);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        
        if (order == null)
        {
            _logger.LogDebug("Order {OrderId} not found", id);
        }
        else
        {
            _logger.LogDebug("Found order {OrderId} for user {UserId} with status {OrderStatus} and total {Total:C}", 
                id, order.UserId, order.Status, order.Total);
        }
        
        return order;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogDebug("Creating new order for user {UserId} with {ItemCount} items", 
            request.UserId, request.Items.Count);
        
        // Validate items
        if (!request.Items.Any())
        {
            _logger.LogWarning("Attempted to create order with no items for user {UserId}", request.UserId);
            throw new InvalidOperationException("Order must contain at least one item");
        }
        
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity {Quantity} for product {ProductId} in order for user {UserId}", 
                    item.Quantity, item.ProductId, request.UserId);
                throw new InvalidOperationException($"Invalid quantity {item.Quantity} for product {item.ProductId}");
            }
            
            if (item.UnitPrice <= 0)
            {
                _logger.LogWarning("Invalid unit price {UnitPrice:C} for product {ProductId} in order for user {UserId}", 
                    item.UnitPrice, item.ProductId, request.UserId);
                throw new InvalidOperationException($"Invalid unit price {item.UnitPrice:C} for product {item.ProductId}");
            }
        }
        
        // Simulate async database operation
        await Task.Delay(150);
        
        var orderItems = request.Items.Select(item => new OrderItem
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();
        
        var order = new Order
        {
            Id = _nextId++,
            UserId = request.UserId,
            Items = orderItems,
            Total = orderItems.Sum(item => item.TotalPrice),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        _orders.Add(order);
        
        _logger.LogInformation("Created order {OrderId} for user {UserId} with {ItemCount} items and total {Total:C}", 
            order.Id, order.UserId, order.Items.Count, order.Total);
        
        // Log detailed item information
        foreach (var item in order.Items)
        {
            _logger.LogDebug("Order {OrderId} item: {ProductName} (ID: {ProductId}) - Qty: {Quantity} x {UnitPrice:C} = {TotalPrice:C}",
                order.Id, item.ProductName, item.ProductId, item.Quantity, item.UnitPrice, item.TotalPrice);
        }
        
        return order;
    }

    public async Task<Order?> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        _logger.LogDebug("Updating order {OrderId} status to {NewStatus}", id, status);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            _logger.LogDebug("Order {OrderId} not found for status update", id);
            return null;
        }
        
        // Validate status transition
        if (!IsValidStatusTransition(order.Status, status))
        {
            _logger.LogWarning("Invalid status transition for order {OrderId}: {CurrentStatus} -> {NewStatus}", 
                id, order.Status, status);
            throw new InvalidOperationException($"Cannot change order status from {order.Status} to {status}");
        }
        
        // Simulate async database operation
        await Task.Delay(100);
        
        var oldStatus = order.Status;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updated order {OrderId} status from {OldStatus} to {NewStatus}", 
            id, oldStatus, status);
        
        return order;
    }

    public async Task<bool> CancelOrderAsync(int id, string reason)
    {
        _logger.LogDebug("Cancelling order {OrderId} with reason: {CancelReason}", id, reason);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            _logger.LogDebug("Order {OrderId} not found for cancellation", id);
            return false;
        }
        
        // Check if order can be cancelled
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            _logger.LogWarning("Cannot cancel order {OrderId} with status {OrderStatus}", id, order.Status);
            throw new InvalidOperationException($"Cannot cancel order with status {order.Status}");
        }
        
        // Simulate async database operation
        await Task.Delay(75);
        
        var oldStatus = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.CancelReason = reason;
        order.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Cancelled order {OrderId} (was {OldStatus}) with reason: {CancelReason}", 
            id, oldStatus, reason);
        
        return true;
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus target)
    {
        return current switch
        {
            OrderStatus.Pending => target is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => target is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => target is OrderStatus.Delivered,
            OrderStatus.Delivered => false, // Cannot change from delivered
            OrderStatus.Cancelled => false, // Cannot change from cancelled
            _ => false
        };
    }
}