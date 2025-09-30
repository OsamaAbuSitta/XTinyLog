using Microsoft.AspNetCore.Mvc;
using XTinyLog.Samples.Web.Models;
using XTinyLog.Samples.Web.Services;

namespace XTinyLog.Samples.Web.Controllers;

/// <summary>
/// Controller demonstrating performance monitoring with XTinyLog
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ApplicationLogger _appLogger;
    private readonly ErrorLogger _errorLogger;

    public OrdersController(
        IOrderService orderService, 
        IPerformanceMonitor performanceMonitor,
        ApplicationLogger appLogger, 
        ErrorLogger errorLogger)
    {
        _orderService = orderService;
        _performanceMonitor = performanceMonitor;
        _appLogger = appLogger;
        _errorLogger = errorLogger;
    }

    /// <summary>
    /// Get orders with performance monitoring
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        using var operation = await _performanceMonitor.StartOperationAsync("GetOrders");
        
        try
        {
            await _appLogger.Logger.DebugAsync("GetOrders endpoint called");
            var orders = await _orderService.GetAllOrdersAsync();
            
            operation.AddMetric("OrderCount", orders.Count());
            await _appLogger.Logger.InfoAsync($"Retrieved {orders.Count()} orders");
            
            return Ok(orders);
        }
        catch (Exception ex)
        {
            operation.SetError(ex);
            await _errorLogger.Logger.ErrorAsync($"Error in GetOrders: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Process order with detailed performance tracking
    /// </summary>
    [HttpPost("process")]
    public async Task<ActionResult<ProcessOrderResult>> ProcessOrder(ProcessOrderRequest request)
    {
        using var operation = await _performanceMonitor.StartOperationAsync("ProcessOrder");
        
        try
        {
            await _appLogger.Logger.InfoAsync($"Processing order for user {request.UserId} with {request.Items.Count} items");
            
            // Simulate validation step
            using (var validationOp = await _performanceMonitor.StartOperationAsync("ValidateOrder"))
            {
                await Task.Delay(50); // Simulate validation time
                validationOp.AddMetric("ItemCount", request.Items.Count);
            }
            
            // Simulate inventory check
            using (var inventoryOp = await _performanceMonitor.StartOperationAsync("CheckInventory"))
            {
                await Task.Delay(100); // Simulate inventory check time
                inventoryOp.AddMetric("ItemsChecked", request.Items.Count);
            }
            
            // Simulate payment processing
            using (var paymentOp = await _performanceMonitor.StartOperationAsync("ProcessPayment"))
            {
                await Task.Delay(200); // Simulate payment processing time
                paymentOp.AddMetric("Amount", request.TotalAmount);
            }
            
            var result = await _orderService.ProcessOrderAsync(request);
            
            operation.AddMetric("OrderId", result.OrderId);
            operation.AddMetric("TotalAmount", request.TotalAmount);
            
            await _appLogger.Logger.InfoAsync($"Successfully processed order {result.OrderId} for ${request.TotalAmount}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            operation.SetError(ex);
            await _errorLogger.Logger.ErrorAsync($"Error processing order for user {request.UserId}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Simulate a slow operation for performance testing
    /// </summary>
    [HttpGet("slow-operation")]
    public async Task<ActionResult<string>> SlowOperation()
    {
        using var operation = await _performanceMonitor.StartOperationAsync("SlowOperation");
        
        try
        {
            await _appLogger.Logger.WarnAsync("Starting intentionally slow operation");
            
            // Simulate slow database query
            await Task.Delay(2000);
            
            operation.AddMetric("DelayMs", 2000);
            await _appLogger.Logger.InfoAsync("Slow operation completed");
            
            return Ok("Slow operation completed");
        }
        catch (Exception ex)
        {
            operation.SetError(ex);
            await _errorLogger.Logger.ErrorAsync($"Error in slow operation: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<object>> GetMetrics()
    {
        try
        {
            await _appLogger.Logger.DebugAsync("GetMetrics endpoint called");
            var metrics = _performanceMonitor.GetMetrics();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            await _errorLogger.Logger.ErrorAsync($"Error getting metrics: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}