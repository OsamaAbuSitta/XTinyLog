namespace XTinyLog.Samples.Web.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ProcessOrderRequest
{
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class ProcessOrderResult
{
    public int OrderId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}