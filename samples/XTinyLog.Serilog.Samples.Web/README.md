# XTinyLog.Serilog Web Sample

This ASP.NET Core web application demonstrates comprehensive usage of the XTinyLog.Serilog sink in a real-world web API scenario.

## Features Demonstrated

### 1. ASP.NET Core Integration
- **Serilog Integration**: Full integration with ASP.NET Core hosting and dependency injection
- **Request Logging**: Automatic HTTP request/response logging with custom enrichment
- **Correlation IDs**: Request correlation tracking across service boundaries
- **Environment-Specific Configuration**: Different logging configurations for Development/Production

### 2. Structured Logging Patterns
- **Context Enrichment**: Adding request-specific context to all log entries
- **Service-Level Logging**: Proper logging patterns in service classes
- **Controller Logging**: Request/response logging in API controllers
- **Error Context**: Rich error logging with business context

### 3. Real-World Scenarios
- **User Management API**: CRUD operations with validation and error handling
- **Order Processing API**: Complex business logic with status transitions
- **Metrics Collection**: Performance and business metrics tracking
- **Exception Handling**: Comprehensive error handling and logging

### 4. Performance Monitoring
- **Request Duration**: Automatic timing of HTTP requests
- **Business Metrics**: Custom metrics for business operations
- **Error Tracking**: Error rates and patterns
- **Resource Usage**: Service-level performance monitoring

## API Endpoints

### Users API (`/api/users`)
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (soft delete)

### Orders API (`/api/orders`)
- `GET /api/orders` - Get all orders (optional user filter)
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}/status` - Update order status
- `POST /api/orders/{id}/cancel` - Cancel order
- `GET /api/orders/metrics` - Get order metrics

### Utility Endpoints
- `GET /` - Welcome message
- `GET /health` - Health check endpoint
- `POST /test-logging` - Test logging functionality
- `GET /swagger` - API documentation (Development only)

## Running the Sample

### Prerequisites
- .NET 8.0 or later
- Visual Studio 2022, VS Code, or command line

### Command Line
```bash
cd samples/XTinyLog.Serilog.Samples.Web
dotnet run
```

The application will start and display the URLs it's listening on (typically `https://localhost:7xxx` and `http://localhost:5xxx`).

### Visual Studio
1. Open the solution file in the root directory
2. Set `XTinyLog.Serilog.Samples.Web` as the startup project
3. Press F5 to run

### Testing the API

#### Using Swagger (Development)
Navigate to `https://localhost:7xxx/swagger` to access the interactive API documentation.

#### Using curl
```bash
# Health check
curl https://localhost:7xxx/health

# Test logging
curl -X POST https://localhost:7xxx/test-logging

# Get users
curl https://localhost:7xxx/api/users

# Create user
curl -X POST https://localhost:7xxx/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com"}'

# Create order
curl -X POST https://localhost:7xxx/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "items": [
      {"productId": 1, "productName": "Widget A", "quantity": 2, "unitPrice": 29.99},
      {"productId": 2, "productName": "Widget B", "quantity": 1, "unitPrice": 49.99}
    ]
  }'

# Get metrics
curl https://localhost:7xxx/api/orders/metrics
```

## Log Output

The application creates HTML log files in the `Logs/` directory:

```
Logs/
└── WebSample-Development/  # Environment-specific directory
    ├── log-20241001.html   # Daily log files
    ├── log-20241002.html
    └── ...
```

### Log Content Examples

#### Request Logging
```
[14:30:15 INF] HTTP GET /api/users responded 200 in 45.2341 ms
Properties:
  RequestHost: localhost:7xxx
  RequestScheme: https
  UserAgent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)
  RemoteIP: ::1
  CorrelationId: 12345678-1234-1234-1234-123456789012
```

#### Service Logging
```
[14:30:15 INF] Retrieved 5 users
Properties:
  CorrelationId: 12345678-1234-1234-1234-123456789012
  SourceContext: XTinyLog.Serilog.Samples.Web.Services.UserService
```

#### Error Logging
```
[14:30:15 ERR] Failed to create user with request {"Name":"Test","Email":"duplicate@example.com"}
System.InvalidOperationException: User with email duplicate@example.com already exists
   at UserService.CreateUserAsync(CreateUserRequest request)
   at UsersController.CreateUser(CreateUserRequest request)
Properties:
  CorrelationId: 12345678-1234-1234-1234-123456789012
  SourceContext: XTinyLog.Serilog.Samples.Web.Controllers.UsersController
```

## Configuration

### Serilog Configuration
The application uses different configurations for different environments:

#### Development
- **Log Level**: Debug and above
- **Console Output**: Detailed formatting with colors
- **HTML Output**: Structured property format for detailed debugging

#### Production
- **Log Level**: Information and above
- **Console Output**: Compact formatting
- **HTML Output**: Inline property format for performance

### XTinyLog Sink Configuration
```csharp
.WriteTo.XTinyLogAsync(config =>
{
    config.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
    config.ApplicationName = $"WebSample-{builder.Environment.EnvironmentName}";
    config.MaxFileSize = 2048; // 2MB
    config.PropertyFormat = PropertyFormatMode.Structured;
})
```

### Request Logging Configuration
```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => /* custom logic */;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) => /* enrichment */;
});
```

## Key Patterns Demonstrated

### 1. Correlation ID Tracking
```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                       ?? Guid.NewGuid().ToString();
    
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        await next();
    }
});
```

### 2. Service-Level Logging
```csharp
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogDebug("Creating new user with email {Email}", request.Email);
        
        try
        {
            // Business logic
            _logger.LogInformation("Created new user {UserId} with name {UserName}", user.Id, user.Name);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
            throw;
        }
    }
}
```

### 3. Controller-Level Context
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<User>> GetUser(int id)
{
    using (LogContext.PushProperty("UserId", id))
    {
        _logger.LogInformation("Getting user {UserId}", id);
        // All logs within this scope will include UserId
    }
}
```

### 4. Metrics Integration
```csharp
public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
{
    var startTime = DateTime.UtcNow;
    var order = await ProcessOrder(request);
    var duration = DateTime.UtcNow - startTime;

    _metricsService.IncrementCounter("orders.created");
    _metricsService.RecordDuration("orders.creation_time", duration);
    
    return order;
}
```

## Performance Considerations

### Async Logging
The sample uses `XTinyLogAsync` for better performance under load:
- Non-blocking I/O operations
- Better throughput for high-volume scenarios
- Proper cancellation token support

### Structured Properties
- **Development**: Uses `PropertyFormatMode.Structured` for detailed debugging
- **Production**: Consider `PropertyFormatMode.Inline` for better performance

### Log Levels
- **Development**: Debug level for comprehensive logging
- **Production**: Information level to reduce noise

## Monitoring and Observability

### Request Correlation
Every request gets a correlation ID that flows through all log entries, making it easy to trace a request through the entire system.

### Business Metrics
The `/api/orders/metrics` endpoint provides insights into:
- Order creation rates
- Processing times
- Error rates
- Status transition patterns

### Error Tracking
Comprehensive error logging includes:
- Full exception details with stack traces
- Business context (user IDs, order IDs, etc.)
- Request correlation information
- Service-level context

## Best Practices Demonstrated

1. **Structured Logging**: Use structured properties for searchable, analyzable logs
2. **Context Enrichment**: Add relevant context to all log entries
3. **Correlation Tracking**: Use correlation IDs to trace requests across services
4. **Error Context**: Include business context when logging errors
5. **Performance Monitoring**: Track both technical and business metrics
6. **Environment-Specific Configuration**: Different logging strategies for different environments
7. **Async Logging**: Use async sinks for better performance
8. **Proper Disposal**: Ensure loggers are properly disposed on application shutdown

## Troubleshooting

### Common Issues

1. **Port Conflicts**: If the default ports are in use, modify `launchSettings.json`
2. **Log Directory Permissions**: Ensure the application can write to the log directory
3. **HTTPS Certificate**: Trust the development certificate with `dotnet dev-certs https --trust`

### Diagnostic Information

- Check the console output for startup information and errors
- Review the HTML log files for detailed request/response information
- Use the `/health` endpoint to verify the application is running
- Check the `/api/orders/metrics` endpoint for performance metrics

## Next Steps

After exploring this sample:

1. **Customize the Configuration**: Modify the Serilog configuration for your needs
2. **Add Authentication**: Integrate with your authentication system for user context
3. **Database Integration**: Replace in-memory storage with actual database operations
4. **Monitoring Integration**: Connect to monitoring systems like Application Insights or Prometheus
5. **Load Testing**: Test the logging performance under realistic load
6. **Production Deployment**: Adapt the configuration for your production environment

## Related Samples

- **Console Sample**: Basic XTinyLog.Serilog usage patterns
- **Advanced Sample**: Performance optimization and custom formatting
- **Main XTinyLog Samples**: Direct XTinyLog usage without Serilog