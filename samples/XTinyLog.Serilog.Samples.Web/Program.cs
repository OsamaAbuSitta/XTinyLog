using Serilog;
using Serilog.Context;
using XTinyLog.Serilog;
using XTinyLog.Serilog.Samples.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.XTinyLogAsync(config =>
        {
            config.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
            config.ApplicationName = $"WebSample-{builder.Environment.EnvironmentName}";
            config.MaxFileSize = 2048; // 2MB
            config.PropertyFormat = PropertyFormatMode.Structured;
        })
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "XTinyLog.Serilog.Web.Sample")
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register sample services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add request logging middleware with custom configuration
app.UseSerilogRequestLogging(options =>
{
    // Customize the request logging template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    
    // Customize log level based on response
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 399
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
    
    // Enrich with additional properties
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
        }
    };
});

// Add correlation ID middleware
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

app.UseAuthorization();
app.MapControllers();

// Add some basic endpoints
app.MapGet("/", () => "XTinyLog.Serilog Web Sample - Check /swagger for API documentation");

app.MapGet("/health", (ILogger<Program> logger) =>
{
    logger.LogInformation("Health check endpoint called");
    return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
});

app.MapPost("/test-logging", (ILogger<Program> logger) =>
{
    var testId = Guid.NewGuid();
    
    logger.LogInformation("Test logging endpoint called with ID {TestId}", testId);
    logger.LogDebug("Debug message for test {TestId}", testId);
    logger.LogWarning("Warning message for test {TestId}", testId);
    
    try
    {
        // Simulate some work that might fail
        if (Random.Shared.Next(1, 5) == 1)
        {
            throw new InvalidOperationException("Simulated error for testing");
        }
        
        logger.LogInformation("Test {TestId} completed successfully", testId);
        return Results.Ok(new { TestId = testId, Status = "Success" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Test {TestId} failed", testId);
        return Results.Problem("Test failed", statusCode: 500);
    }
});

// Log application startup
Log.Information("Starting XTinyLog.Serilog Web Sample");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Content Root: {ContentRoot}", app.Environment.ContentRootPath);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}