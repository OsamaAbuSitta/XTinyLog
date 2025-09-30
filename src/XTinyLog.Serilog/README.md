# XTinyLog.Serilog

[![NuGet Version](https://img.shields.io/nuget/v/XTinyLog.Serilog.svg)](https://www.nuget.org/packages/XTinyLog.Serilog/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/XTinyLog.Serilog.svg)](https://www.nuget.org/packages/XTinyLog.Serilog/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A Serilog sink that outputs logs in XTinyLog's interactive HTML format with responsive design, dark/light themes, and rich formatting capabilities.

## ✨ Features

- 🎨 **Beautiful HTML Output** - Leverage XTinyLog's responsive design with dark/light themes
- 🔌 **Seamless Integration** - Drop-in replacement for existing Serilog sinks
- ⚡ **Async Support** - Both synchronous and asynchronous logging capabilities
- 🏗️ **Structured Logging** - Full support for Serilog's structured logging features
- ⚙️ **Flexible Configuration** - Configure via code or appsettings.json
- 🎯 **Multi-Framework** - Supports .NET Framework 4.6.1, .NET Standard 2.0, .NET 6.0, and .NET 8.0

## 🚀 Quick Start

### Installation

Install XTinyLog.Serilog via NuGet Package Manager:

```bash
dotnet add package XTinyLog.Serilog
```

Or via Package Manager Console:

```powershell
PM> Install-Package XTinyLog.Serilog
```

### Basic Usage

```csharp
using Serilog;
using XTinyLog.Serilog;

// Configure Serilog with XTinyLog sink
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();

// Use Serilog as usual
Log.Information("Application started successfully");
Log.Debug("Processing user request for {UserId}", userId);
Log.Warning("Configuration file not found, using defaults");
Log.Error(ex, "Failed to connect to database");
```

### Custom Configuration

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"D:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
        config.MaxFileSize = 5 * 1024 * 1024; // 5MB
        config.IncludeProperties = true;
    })
    .CreateLogger();
```

### Async Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogAsync(config =>
    {
        config.LogDirectory = @"D:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
    })
    .CreateLogger();

// Serilog handles async internally
Log.Information("This will be logged asynchronously");
```

## ⚙️ Configuration

### Configuration via Code

#### Using Action Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"D:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
        config.FileName = () => $"log_{DateTime.Now:yyyy-MM-dd}.html";
        config.MaxFileSize = 10 * 1024 * 1024; // 10MB
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();
```

#### Using Builder Pattern

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(builder => builder
        .SetLogDirectory(@"D:\MyApp\Logs")
        .SetApplicationName("MyApplication")
        .SetMaxFileSize(10 * 1024 * 1024)
        .IncludeStructuredProperties()
        .SetPropertyFormat(PropertyFormatMode.Inline)
    )
    .CreateLogger();
```

### Configuration via appsettings.json

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "XTinyLog",
        "Args": {
          "LogDirectory": "D:\\MyApp\\Logs",
          "ApplicationName": "MyApplication",
          "MaxFileSize": 10485760,
          "IncludeProperties": true,
          "PropertyFormat": "Structured"
        }
      }
    ]
  }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LogDirectory` | `string` | `"C:\\"` | Directory where log files are stored |
| `ApplicationName` | `string` | Current month/year | Subdirectory name for organizing logs |
| `FileName` | `Func<string>` | Current date | HTML file name pattern |
| `MaxFileSize` | `long` | `1,048,576` (1MB) | Maximum file size before rotation |
| `IncludeProperties` | `bool` | `true` | Include structured properties in output |
| `PropertyFormat` | `PropertyFormatMode` | `Inline` | How to format structured properties |

### Property Format Modes

- **`Inline`** - Properties rendered inline with the message
- **`Structured`** - Properties rendered as structured data
- **`Hidden`** - Properties not included in output

## 📚 Advanced Examples

### Structured Logging

```csharp
// Properties will be included in HTML output
Log.Information("User {UserId} logged in from {IPAddress} at {LoginTime}", 
    userId, ipAddress, DateTime.Now);

// With complex objects
Log.Information("Processing order {@Order} for customer {CustomerId}", 
    order, customerId);
```

### Exception Logging

```csharp
try
{
    // Your code here
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to process order {OrderId} for customer {CustomerId}", 
        orderId, customerId);
}
```

### Multiple Sinks

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs\HTML";
        config.ApplicationName = "WebAPI";
    })
    .WriteTo.File("logs/app.log")
    .CreateLogger();
```

### ASP.NET Core Integration

```csharp
// In Program.cs
using Serilog;
using XTinyLog.Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.XTinyLog(config =>
        {
            config.LogDirectory = @"C:\WebApp\Logs";
            config.ApplicationName = "WebApplication";
        });
});

var app = builder.Build();

// Use Serilog for request logging
app.UseSerilogRequestLogging();
```

## 🎨 HTML Output Features

The XTinyLog sink generates the same beautiful HTML output as the core XTinyLog library:

- **🔍 Real-time Search** - Filter logs as you type
- **🎯 Level Filtering** - Show/hide specific log levels  
- **📅 Date Range** - Filter by time periods
- **🌙 Theme Toggle** - Switch between light and dark themes
- **📱 Responsive Design** - Works on desktop, tablet, and mobile
- **⌨️ Keyboard Shortcuts** - Navigate efficiently with hotkeys
- **📤 Export Options** - Export to JSON, CSV formats

## 🔧 Troubleshooting

### Common Issues

**Q: Logs are not appearing in HTML files**
A: Ensure the application has write permissions to the specified log directory and that the XTinyLog sink is properly configured.

**Q: Structured properties are not showing**
A: Make sure `IncludeProperties` is set to `true` in your configuration.

**Q: Performance issues with high-volume logging**
A: Use the async sink (`WriteTo.XTinyLogAsync()`) and consider increasing `MaxFileSize` to reduce file rotation frequency.

### Debug Configuration

Enable Serilog's self-logging to diagnose issues:

```csharp
using Serilog.Debugging;

// Enable self-logging
SelfLog.Enable(Console.Error);

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();
```

## 🔗 Related Packages

- **[XTinyLog](https://www.nuget.org/packages/XTinyLog/)** - Core XTinyLog library
- **[Serilog](https://www.nuget.org/packages/Serilog/)** - Serilog logging library

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## 🙏 Acknowledgments

- Built on top of the excellent [Serilog](https://serilog.net/) logging library
- Uses [XTinyLog](https://github.com/OsamaAbuSitta/XTinyLog) for HTML generation

---

**Made with ❤️ by the XTinyLog team**