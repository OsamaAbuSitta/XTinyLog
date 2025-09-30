# XTinyLog

[![Build Status](https://github.com/OsamaAbuSitta/XTinyLog/workflows/CI/badge.svg)](https://github.com/OsamaAbuSitta/XTinyLog/actions)
[![NuGet Version](https://img.shields.io/nuget/v/XTinyLog.svg)](https://www.nuget.org/packages/XTinyLog/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/XTinyLog.svg)](https://www.nuget.org/packages/XTinyLog/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-4.6.1%20%7C%20Standard%202.0%20%7C%206.0%20%7C%208.0-blue)](https://dotnet.microsoft.com/)

A modern, lightweight C# logging library that generates beautiful HTML log files with responsive design, dark/light themes, and interactive features. Perfect for applications that need visual log analysis and debugging capabilities.

## ✨ Features

- 🎨 **Beautiful HTML Output** - Responsive design with dark/light themes
- 🔍 **Interactive Features** - Real-time filtering, search, and export capabilities
- 🚀 **High Performance** - Async/await patterns with thread-safe operations
- 📱 **Mobile Friendly** - Responsive design that works on all devices
- ⚙️ **Easy Configuration** - Fluent API with sensible defaults
- 🎯 **Multi-Framework** - Supports .NET Framework 4.6.1, .NET Standard 2.0, .NET 6.0, and .NET 8.0
- 🔒 **Thread Safe** - Concurrent logging without performance penalties
- 📦 **Zero Dependencies** - Lightweight with no external dependencies

## 📸 Screenshots

### Light Theme
![Light Theme Screenshot](assets/screenshot-light.png)

### Dark Theme  
![Dark Theme Screenshot](assets/screenshot-dark.png)

### Mobile View
![Mobile View Screenshot](assets/screenshot-mobile.png)

## 🚀 Quick Start

### Installation

Install XTinyLog via NuGet Package Manager:

```bash
dotnet add package XTinyLog
```

Or via Package Manager Console:

```powershell
PM> Install-Package XTinyLog
```

### Basic Usage

```csharp
using XTinyLog;

// Simple logging with default configuration
XLog.Log.Info("Application started successfully");
XLog.Log.Debug("Processing user request");
XLog.Log.Warn("Configuration file not found, using defaults");
XLog.Log.Error("Failed to connect to database");

// Exception logging
try 
{
    // Your code here
}
catch (Exception ex)
{
    XLog.Log.Error(ex);
}
```

### Async Logging

```csharp
using XTinyLog;

// Async logging for better performance
await XLog.Log.InfoAsync("Application started successfully");
await XLog.Log.DebugAsync("Processing user request");
await XLog.Log.WarnAsync("Configuration file not found");
await XLog.Log.ErrorAsync("Database connection failed");

// With cancellation token
var cts = new CancellationTokenSource();
await XLog.Log.InfoAsync("Processing...", cts.Token);
```

## ⚙️ Configuration

### Default Configuration

XTinyLog works out of the box with sensible defaults:

```csharp
// Uses default configuration
XLog.Log.Info("This will be logged to C:\\[Month Year]\\[dd MM yyyy].html");
```

### Custom Configuration

#### Using Action Configuration

```csharp
var customLog = XLog.Create(config =>
{
    config.LogDirectory = @"D:\MyApp\Logs";
    config.ApplicationName = () => "MyApplication";
    config.FileName = () => $"log_{DateTime.Now:yyyy-MM-dd}.html";
    config.MaxFileSize = 5 * 1024 * 1024; // 5MB
});

customLog.Info("Custom configured log entry");
```

#### Using Fluent Builder API

```csharp
var fluentLog = XLog.Create(builder => builder
    .SetLogDirectory(@"D:\MyApp\Logs")
    .SetApplicationName("MyApplication")
    .SetFileName(() => $"app_{DateTime.Now:yyyy-MM-dd}.html")
    .SetMaxFileSize(10 * 1024 * 1024) // 10MB
    .EnableDebugMode()
);

fluentLog.Info("Fluent API configured log entry");
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LogDirectory` | `string` | `"C:\\"` | Directory where log files are stored |
| `ApplicationName` | `Func<string>` | Current month/year | Subdirectory name for organizing logs |
| `FileName` | `Func<string>` | Current date | HTML file name pattern |
| `MaxFileSize` | `long` | `1,048,576` (1MB) | Maximum file size before rotation |

## 🎨 HTML Features

### Interactive Elements

- **🔍 Real-time Search** - Filter logs as you type
- **🎯 Level Filtering** - Show/hide specific log levels
- **📅 Date Range** - Filter by time periods
- **🌙 Theme Toggle** - Switch between light and dark themes
- **📱 Responsive Design** - Works on desktop, tablet, and mobile
- **⌨️ Keyboard Shortcuts** - Navigate efficiently with hotkeys
- **📤 Export Options** - Export to JSON, CSV formats

### Visual Enhancements

- **Color-coded log levels** with distinct styling
- **Collapsible entries** for better organization
- **Smooth animations** and transitions
- **Typography optimized** for readability
- **Syntax highlighting** for structured data

## 📚 Advanced Examples

### Multiple Logger Instances

```csharp
// Create separate loggers for different components
var apiLogger = XLog.Create(config =>
{
    config.LogDirectory = @"C:\Logs\API";
    config.ApplicationName = () => "WebAPI";
});

var dbLogger = XLog.Create(config =>
{
    config.LogDirectory = @"C:\Logs\Database";
    config.ApplicationName = () => "DatabaseLayer";
});

apiLogger.Info("API request received");
dbLogger.Debug("Database query executed");
```

### ASP.NET Core Integration

```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register XTinyLog as a singleton
    services.AddSingleton<XLog>(provider => 
        XLog.Create(config =>
        {
            config.LogDirectory = @"C:\WebApp\Logs";
            config.ApplicationName = () => "WebApplication";
        }));
}

// In your controllers or services
public class HomeController : Controller
{
    private readonly XLog _logger;
    
    public HomeController(XLog logger)
    {
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        await _logger.InfoAsync("Home page accessed");
        return View();
    }
}
```

### Performance Monitoring

```csharp
var performanceLog = XLog.Create(builder => builder
    .SetLogDirectory(@"C:\Performance")
    .SetApplicationName("PerformanceMonitor")
    .SetMaxFileSize(50 * 1024 * 1024) // 50MB for high-volume logging
);

// Log performance metrics
var stopwatch = Stopwatch.StartNew();
// ... your operation
stopwatch.Stop();

await performanceLog.InfoAsync($"Operation completed in {stopwatch.ElapsedMilliseconds}ms");
```

## 🔧 Troubleshooting

### Common Issues

**Q: Log files are not being created**
A: Ensure the application has write permissions to the specified log directory.

**Q: HTML output is not displaying correctly**
A: Check that the HTML file is not corrupted and that your browser supports modern CSS features.

**Q: Performance issues with high-volume logging**
A: Use async methods (`InfoAsync`, `ErrorAsync`, etc.) and consider increasing `MaxFileSize` to reduce file rotation frequency.

### Debug Mode

Enable debug mode to get detailed error information:

```csharp
// Add DEBUG_LOG conditional compilation symbol
// Or use fluent API
var debugLog = XLog.Create(builder => builder
    .EnableDebugMode()
    .SetLogDirectory(@"C:\Debug\Logs")
);
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8.0 SDK
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet test`

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/XTinyLog/)
- [GitHub Repository](https://github.com/OsamaAbuSitta/XTinyLog)
- [Documentation](https://github.com/OsamaAbuSitta/XTinyLog/wiki)
- [Issue Tracker](https://github.com/OsamaAbuSitta/XTinyLog/issues)
- [Changelog](CHANGELOG.md)

## 🙏 Acknowledgments

- Thanks to all contributors who have helped improve XTinyLog
- Inspired by modern web design principles and developer experience best practices

---

**Made with ❤️ by the XTinyLog team**