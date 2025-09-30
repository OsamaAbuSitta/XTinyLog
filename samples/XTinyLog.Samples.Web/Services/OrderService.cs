using XTinyLog.Samples.Web.Models;

namespace XTinyLog.Samples.Web.Services;

/// <summary>
/// Order service demonstrating performance monitoring with XTinyLog
/// </summary>
public class OrderService : IOrderService
{
    private readonly ApplicationLogger _appLogger;
    private readonly PerformanceLogger _performanceLogger;
    private readonly ErrorLogger _errorLogger;
    private static readonly List<Order> _orders = new();
    private static int _nextId = 1;

    public OrderService(ApplicationLogger appLogger, PerformanceLogger performanceLogger, ErrorLogger errorLogger)
    {
        _appLogger = appLogger;
        _performanceLogger = performanceLogger;
        _errorLogger = errorLogger;
        
        // Initialize with sample data
        if (!_orders.Any())
        {
            InitializeSampleData();
        }
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        var startTime = DateTime.UtcNow;
        await _appLogger.Logger.DebugAsync("OrderService.GetAllOrdersAsync called");
        
        try
        {
            // Simulate database query delay
            await Task.Delay(150);
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _performanceLogger.Logger.InfoAsync($"GetAllOrdersAsync completed in {duration:F2}ms, returned {_orders.Count} orders");
            
            return _orders.ToList();
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _errorLogger.Logger.ErrorAsync($"GetAllOrdersAsync failed after {duration:F2}ms: {ex.Message}");
            throw;
        }
    }

    public async Task<ProcessOrderResult> ProcessOrderAsync(ProcessOrderRequest request)
    {
        var startTime = DateTime.UtcNow;
        await _appLogger.Logger.InfoAsync($"OrderService.ProcessOrderAsync called for user {request.UserId}");
        
        try
        {
            // Step 1: Validate order
            await _performanceLogger.Logger.DebugAsync("Starting order validation");
            await ValidateOrderAsync(request);
            var validationTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _performanceLogger.Logger.InfoAsync($"Order validation completed in {validationTime:F2}ms");
            
            // Step 2: Check inventory
            var inventoryStart = DateTime.UtcNow;
            await _performanceLogger.Logger.DebugAsync("Starting inventory check");
            await CheckInventoryAsync(request.Items);
            var inventoryTime = (DateTime.UtcNow - inventoryStart).TotalMilliseconds;
            await _performanceLogger.Logger.InfoAsync($"Inventory check completed in {inventoryTime:F2}ms");
            
            // Step 3: Process payment
            var paymentStart = DateTime.UtcNow;
            await _performanceLogger.Logger.DebugAsync($"Starting payment processing for ${request.TotalAmount}");
            await ProcessPaymentAsync(request.TotalAmount);
            var paymentTime = (DateTime.UtcNow - paymentStart).TotalMilliseconds;
            await _performanceLogger.Logger.InfoAsync($"Payment processing completed in {paymentTime:F2}ms");
            
            // Step 4: Create order
            var orderStart = DateTime.UtcNow;
            var order = await CreateOrderAsync(request);
            var orderTime = (DateTime.UtcNow - orderStart).TotalMilliseconds;
            await _performanceLogger.Logger.InfoAsync($"Order creation completed in {orderTime:F2}ms");
            
            var totalTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _appLogger.Logger.InfoAsync($"Order {order.Id} processed successfully in {totalTime:F2}ms");
            await _performanceLogger.Logger.InfoAsync($"ProcessOrderAsync total time: {totalTime:F2}ms (validation: {validationTime:F2}ms, inventory: {inventoryTime:F2}ms, payment: {paymentTime:F2}ms, creation: {orderTime:F2}ms)");
            
            return new ProcessOrderResult
            {
                OrderId = order.Id,
                Success = true,
                Message = "Order processed successfully"
            };
        }
        catch (Exception ex)
        {
            var totalTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await _errorLogger.Logger.ErrorAsync($"ProcessOrderAsync failed after {totalTime:F2}ms for user {request.UserId}: {ex.Message}");
            
            return new ProcessOrderResult
            {
                OrderId = 0,
                Success = false,
                Message = ex.Message
            };
        }
    }

    private async Task ValidateOrderAsync(ProcessOrderRequest request)
    {
        // Simulate validation logic
        await Task.Delay(50);
        
        if (request.UserId <= 0)
        {
            throw new ArgumentException("Invalid user ID");
        }
        
        if (!request.Items.Any())
        {
            throw new ArgumentException("Order must contain at least one item");
        }
        
        if (request.TotalAmount <= 0)
        {
            throw new ArgumentException("Total amount must be greater than zero");
        }
        
        await _appLogger.Logger.DebugAsync($"Order validation passed for user {request.UserId}");
    }

    private async Task CheckInventoryAsync(List<OrderItem> items)
    {
        // Simulate inventory check
        await Task.Delay(100);
        
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"Invalid quantity for product {item.ProductId}");
            }
            
            // Simulate random inventory shortage (10% chance)
            if (Random.Shared.Next(1, 11) == 1)
            {
                throw new InvalidOperationException($"Insufficient inventory for product {item.ProductId}");
            }
        }
        
        await _appLogger.Logger.DebugAsync($"Inventory check passed for {items.Count} items");
    }

    private async Task ProcessPaymentAsync(decimal amount)
    {
        // Simulate payment processing
        await Task.Delay(200);
        
        // Simulate random payment failure (5% chance)
        if (Random.Shared.Next(1, 21) == 1)
        {
            throw new InvalidOperationException("Payment processing failed");
        }
        
        await _appLogger.Logger.DebugAsync($"Payment processed successfully for ${amount}");
    }

    private async Task<Order> CreateOrderAsync(ProcessOrderRequest request)
    {
        // Simulate order creation
        await Task.Delay(75);
        
        var order = new Order
        {
            Id = _nextId++,
            UserId = request.UserId,
            TotalAmount = request.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.ToList()
        };
        
        _orders.Add(order);
        
        await _appLogger.Logger.InfoAsync($"Created order {order.Id} for user {request.UserId}");
        return order;
    }

    private void InitializeSampleData()
    {
        _orders.AddRange(new[]
        {
            new Order
            {
                Id = _nextId++,
                UserId = 1,
                TotalAmount = 99.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Items = new List<OrderItem>
                {
                    new() { ProductId = 1, ProductName = "Widget A", Quantity = 2, Price = 49.99m }
                }
            },
            new Order
            {
                Id = _nextId++,
                UserId = 2,
                TotalAmount = 149.98m,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Items = new List<OrderItem>
                {
                    new() { ProductId = 2, ProductName = "Widget B", Quantity = 1, Price = 74.99m },
                    new() { ProductId = 3, ProductName = "Widget C", Quantity = 1, Price = 74.99m }
                }
            }
        });
    }
}