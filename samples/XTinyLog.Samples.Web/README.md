# XTinyLog Web Application Sample

This ASP.NET Core web application demonstrates advanced XTinyLog integration patterns including dependency injection, middleware, performance monitoring, and structured logging across different application layers.

## Features Demonstrated

### 1. Dependency Injection Integration
- Multiple logger instances for different purposes (Application, API, Performance, Error)
- Service registration and configuration
- Logger injection into controllers and services

### 2. Request Logging Middleware
- Automatic HTTP request/response logging
- Performance tracking for all endpoints
- Request correlation with unique IDs
- Slow request detection and alerting

### 3. Performance Monitoring
- Operation-level performance tracking
- Custom metrics collection
- Performance analytics and reporting
- Integration with XTinyLog for performance logs

### 4. Structured Logging Patterns
- Component-specific logging (Users, Orders, etc.)
- Error handling and exception logging
- Business logic logging in services
- API endpoint logging in controllers

### 5. Real-world Scenarios
- User management operations
- Order processing with validation steps
- Error simulation and handling
- Performance bottleneck identification

## Project Structure

```
XTinyLog.Samples.Web/
├── Controllers/
│   ├── UsersController.cs      # User management API
│   └── OrdersController.cs     # Order processing with performance monitoring
├── Middleware/
│   └── RequestLoggingMiddleware.cs  # HTTP request logging
├── Models/
│   ├── User.cs                 # User-related models
│   └── Order.cs                # Order-related models
├── Services/
│   ├── UserService.cs          # Business logic with logging
│   ├── OrderService.cs         # Performance-monitored operations
│   └── PerformanceMonitor.cs   # Custom performance tracking
└── Program.cs                  # DI configuration and startup
```

## Running the Sample

```bash
cd samples/XTinyLog.Samples.Web
dotnet run
```

The application will start on `https://localhost:7xxx` and `http://localhost:5xxx`.

## API Endpoints

### Users API
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Orders API
- `GET /api/orders` - Get all orders
- `POST /api/orders/process` - Process new order (with performance tracking)
- `GET /api/orders/slow-operation` - Simulate slow operation
- `GET /api/orders/metrics` - Get performance metrics

## Log Output Locations

The application creates separate log directories for different purposes:

- **Application Logs**: `./logs/application/` - General application events
- **API Logs**: `./logs/api/` - HTTP request/response logging
- **Performance Logs**: `./logs/performance/` - Performance metrics and monitoring
- **Error Logs**: `./logs/errors/` - Exceptions and error conditions

## Key Integration Patterns

### 1. Logger Registration in DI Container

```csharp
services.AddSingleton<ApplicationLogger>(provider =>
{
    var logger = XLog.Create(config =>
    {
        config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "application");
        config.ApplicationName = "WebApp";
        config.FileName = () => $"app_{DateTime.Now:yyyy-MM-dd}";
        config.MaxFileSize = 5000;
    });
    return new ApplicationLogger(logger);
});
```

### 2. Middleware Integration

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var stopwatch = Stopwatch.StartNew();
    var requestId = Guid.NewGuid().ToString("N")[..8];
    
    await _apiLogger.Logger.InfoAsync($"[{requestId}] {context.Request.Method} {context.Request.Path} - Request started");
    
    // Process request...
    
    await _apiLogger.Logger.InfoAsync($"[{requestId}] Response: {context.Response.StatusCode} ({duration}ms)");
}
```

### 3. Service Layer Logging

```csharp
public async Task<User> CreateUserAsync(string name, string email)
{
    await _appLogger.Logger.InfoAsync($"Creating user: {name}");
    
    try
    {
        // Business logic...
        await _appLogger.Logger.InfoAsync($"Created user: {user.Name} (ID: {user.Id})");
        return user;
    }
    catch (Exception ex)
    {
        await _errorLogger.Logger.ErrorAsync($"Error creating user {name}: {ex.Message}");
        throw;
    }
}
```

### 4. Performance Monitoring

```csharp
public async Task<ActionResult> ProcessOrder(ProcessOrderRequest request)
{
    using var operation = await _performanceMonitor.StartOperationAsync("ProcessOrder");
    
    try
    {
        var result = await _orderService.ProcessOrderAsync(request);
        operation.AddMetric("OrderId", result.OrderId);
        return Ok(result);
    }
    catch (Exception ex)
    {
        operation.SetError(ex);
        throw;
    }
}
```

## Testing the Application

### 1. Basic User Operations
```bash
# Get all users
curl https://localhost:7xxx/api/users

# Create a user
curl -X POST https://localhost:7xxx/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com"}'
```

### 2. Performance Testing
```bash
# Process an order (with performance tracking)
curl -X POST https://localhost:7xxx/api/orders/process \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "totalAmount": 99.99,
    "items": [
      {"productId": 1, "productName": "Test Product", "quantity": 2, "price": 49.99}
    ]
  }'

# Trigger slow operation
curl https://localhost:7xxx/api/orders/slow-operation

# Get performance metrics
curl https://localhost:7xxx/api/orders/metrics
```

## Expected Log Output

Each log file will contain rich HTML output with:

- **Request Correlation**: Each request gets a unique ID for tracing
- **Performance Metrics**: Detailed timing information for operations
- **Error Context**: Full exception details with stack traces
- **Business Events**: User creation, order processing, etc.
- **Interactive Features**: Search, filtering, dark/light themes

## Learning Objectives

1. **Dependency Injection**: How to register and inject XTinyLog instances
2. **Middleware Integration**: Automatic request logging across all endpoints
3. **Performance Monitoring**: Custom performance tracking with detailed metrics
4. **Error Handling**: Comprehensive error logging and exception management
5. **Separation of Concerns**: Different loggers for different purposes
6. **Async Patterns**: Proper async logging throughout the application
7. **Real-world Usage**: Practical logging patterns for web applications