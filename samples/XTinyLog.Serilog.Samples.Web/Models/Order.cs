namespace XTinyLog.Serilog.Samples.Web.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CancelReason { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class CreateOrderRequest
{
    public int UserId { get; set; }
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}