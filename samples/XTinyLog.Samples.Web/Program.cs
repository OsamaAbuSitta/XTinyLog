using XTinyLog;
using XTinyLog.Samples.Web.Services;

namespace XTinyLog.Samples.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure XTinyLog instances for dependency injection
        ConfigureLogging(builder.Services);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Add custom services that use logging
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Add custom middleware for request logging
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.MapControllers();

        // Log application startup
        var appLogger = app.Services.GetRequiredService<ApplicationLogger>();
        appLogger.Logger.Info("Web application started successfully");

        app.Run();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        // Register different logger instances for different purposes
        
        // Application-wide logger
        services.AddSingleton<ApplicationLogger>(provider =>
        {
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "application");
                config.ApplicationName = "WebApp";
                config.FileName = () => $"app_{DateTime.Now:yyyy-MM-dd}";
                config.MaxFileSize = 5000; // 5MB
            });
            return new ApplicationLogger(logger);
        });

        // API request/response logger
        services.AddSingleton<ApiLogger>(provider =>
        {
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "api");
                config.ApplicationName = "API";
                config.FileName = () => $"api_{DateTime.Now:yyyy-MM-dd}";
                config.MaxFileSize = 10000; // 10MB
            });
            return new ApiLogger(logger);
        });

        // Performance monitoring logger
        services.AddSingleton<PerformanceLogger>(provider =>
        {
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "performance");
                config.ApplicationName = "Performance";
                config.FileName = () => $"perf_{DateTime.Now:yyyy-MM-dd}";
                config.MaxFileSize = 2000; // 2MB
            });
            return new PerformanceLogger(logger);
        });

        // Error and exception logger
        services.AddSingleton<ErrorLogger>(provider =>
        {
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs", "errors");
                config.ApplicationName = "Errors";
                config.FileName = () => $"error_{DateTime.Now:yyyy-MM-dd}";
                config.MaxFileSize = 3000; // 3MB
            });
            return new ErrorLogger(logger);
        });
    }
}

// Wrapper classes for different logger types to enable dependency injection
public class ApplicationLogger
{
    public XLog Logger { get; }
    public ApplicationLogger(XLog logger) => Logger = logger;
}

public class ApiLogger
{
    public XLog Logger { get; }
    public ApiLogger(XLog logger) => Logger = logger;
}

public class PerformanceLogger
{
    public XLog Logger { get; }
    public PerformanceLogger(XLog logger) => Logger = logger;
}

public class ErrorLogger
{
    public XLog Logger { get; }
    public ErrorLogger(XLog logger) => Logger = logger;
}