using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using XTinyLog.Serilog.Samples.Web.Models;
using XTinyLog.Serilog.Samples.Web.Services;

namespace XTinyLog.Serilog.Samples.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrderService _orderService;
    private readonly IMetricsService _metricsService;

    public OrdersController(
        ILogger<OrdersController> logger, 
        IOrderService orderService,
        IMetricsService metricsService)
    {
        _logger = logger;
        _orderService = orderService;
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] int? userId = null)
    {
        using (LogContext.PushProperty("RequestedUserId", userId))
        {
            _logger.LogInformation("Getting orders for user filter {UserId}", userId);

            try
            {
                var orders = await _orderService.GetOrdersAsync(userId);
                _logger.LogInformation("Retrieved {OrderCount} orders", orders.Count());
                
                _metricsService.IncrementCounter("orders.retrieved", orders.Count());
                
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve orders for user filter {UserId}", userId);
                _metricsService.IncrementCounter("orders.retrieval_errors");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        using (LogContext.PushProperty("OrderId", id))
        {
            _logger.LogInformation("Getting order {OrderId}", id);

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved order {OrderId} for user {UserId} with total {Total:C}", 
                    id, order.UserId, order.Total);
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        var operationId = Guid.NewGuid();
        
        using (LogContext.PushProperty("OperationId", operationId))
        using (LogContext.PushProperty("UserId", request.UserId))
        {
            _logger.LogInformation("Creating new order for user {UserId} with {ItemCount} items", 
                request.UserId, request.Items?.Count ?? 0);

            try
            {
                // Validate request
                if (request.UserId <= 0 || request.Items == null || !request.Items.Any())
                {
                    _logger.LogWarning("Invalid order creation request: {@Request}", request);
                    return BadRequest("UserId and Items are required");
                }

                var startTime = DateTime.UtcNow;
                var order = await _orderService.CreateOrderAsync(request);
                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation("Successfully created order {OrderId} for user {UserId} with total {Total:C} in {Duration}ms", 
                    order.Id, order.UserId, order.Total, duration.TotalMilliseconds);

                _metricsService.IncrementCounter("orders.created");
                _metricsService.RecordDuration("orders.creation_time", duration);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order creation failed due to business rule violation for user {UserId}", request.UserId);
                _metricsService.IncrementCounter("orders.creation_business_errors");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order for user {UserId} with request {@Request}", request.UserId, request);
                _metricsService.IncrementCounter("orders.creation_errors");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<Order>> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        using (LogContext.PushProperty("OrderId", id))
        {
            _logger.LogInformation("Updating order {OrderId} status to {NewStatus}", id, request.Status);

            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                if (order == null)
                {
                    _logger.LogWarning("Attempted to update status of non-existent order {OrderId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully updated order {OrderId} status from {OldStatus} to {NewStatus}", 
                    id, order.Status, request.Status);

                _metricsService.IncrementCounter($"orders.status_updates.{request.Status.ToString().ToLower()}");

                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order status update failed due to business rule violation for order {OrderId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update order {OrderId} status to {Status}", id, request.Status);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelOrder(int id, [FromBody] CancelOrderRequest? request = null)
    {
        using (LogContext.PushProperty("OrderId", id))
        {
            var reason = request?.Reason ?? "No reason provided";
            _logger.LogInformation("Cancelling order {OrderId} with reason: {CancelReason}", id, reason);

            try
            {
                var success = await _orderService.CancelOrderAsync(id, reason);
                if (!success)
                {
                    _logger.LogWarning("Attempted to cancel non-existent or non-cancellable order {OrderId}", id);
                    return NotFound("Order not found or cannot be cancelled");
                }

                _logger.LogInformation("Successfully cancelled order {OrderId} with reason: {CancelReason}", id, reason);
                _metricsService.IncrementCounter("orders.cancelled");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order cancellation failed due to business rule violation for order {OrderId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel order {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    [HttpGet("metrics")]
    public ActionResult GetMetrics()
    {
        _logger.LogInformation("Retrieving order metrics");
        
        try
        {
            var metrics = _metricsService.GetMetrics();
            _logger.LogDebug("Retrieved {MetricCount} metrics", metrics.Count);
            
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve metrics");
            return StatusCode(500, "Internal server error");
        }
    }
}