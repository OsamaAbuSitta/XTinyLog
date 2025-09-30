# Changelog - XTinyLog.Serilog

All notable changes to the XTinyLog Serilog Sink will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2025-09-30

### Added
- Initial release of XTinyLog Serilog Sink
- Synchronous sink implementation (`XTinyLogSink`) with `ILogEventSink` interface
- Asynchronous sink implementation (`XTinyLogAsyncSink`) with `IAsyncLogEventSink` interface
- Comprehensive configuration system with `XTinyLogSinkConfiguration` class
- Fluent configuration API with `XTinyLogSinkConfigurationBuilder`
- Serilog extension methods for easy integration (`WriteTo.XTinyLog()`, `WriteTo.XTinyLogAsync()`)
- Support for appsettings.json configuration
- Structured logging property support with multiple formatting modes
- Log level mapping from Serilog to XTinyLog levels
- Exception formatting with full stack trace support
- Message template rendering with property value substitution
- Comprehensive error handling without throwing exceptions to application
- Multi-framework support (.NET Framework 4.6.1, .NET Standard 2.0, .NET 6.0, .NET 8.0)
- Thread-safe logging operations
- XML documentation for all public APIs
- Comprehensive test suite with unit and integration tests

### Features
- **Seamless Integration**: Drop-in replacement for existing Serilog sinks
- **Beautiful HTML Output**: Leverages XTinyLog's responsive design with dark/light themes
- **Structured Logging**: Full support for Serilog's structured logging features
- **Flexible Configuration**: Configure via code or appsettings.json
- **Async Support**: Both synchronous and asynchronous logging capabilities
- **Property Formatting**: Multiple modes for displaying structured properties
- **Error Resilience**: Graceful error handling with fallback mechanisms

### Configuration Options
- `LogDirectory`: Directory where log files are stored
- `ApplicationName`: Application name for log organization
- `FileName`: Custom file naming function
- `MaxFileSize`: Maximum file size before rotation
- `IncludeProperties`: Include structured properties in output
- `PropertyFormat`: How to format structured properties (Inline, Structured, Hidden)

### Supported Frameworks
- .NET Framework 4.6.1
- .NET Standard 2.0
- .NET 6.0
- .NET 8.0

### Dependencies
- Serilog 4.0.2+
- XTinyLog 2.0.0+

---

## Migration Guide

### From Other Serilog Sinks

If you're migrating from another Serilog sink to XTinyLog.Serilog:

#### 1. Install the Package

```bash
dotnet add package XTinyLog.Serilog
```

#### 2. Update Configuration

Replace your existing sink configuration:

```csharp
// Before (example with File sink)
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/app.log")
    .CreateLogger();

// After (with XTinyLog sink)
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs";
        config.ApplicationName = "MyApp";
    })
    .CreateLogger();
```

#### 3. Update appsettings.json (if applicable)

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "XTinyLog",
        "Args": {
          "LogDirectory": "C:\\Logs",
          "ApplicationName": "MyApp",
          "MaxFileSize": 10485760,
          "IncludeProperties": true
        }
      }
    ]
  }
}
```

### Configuration Examples

#### Basic Usage
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();
```

#### Custom Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"D:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
        config.MaxFileSize = 5 * 1024 * 1024; // 5MB
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();
```

#### Async Logging
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogAsync(config =>
    {
        config.LogDirectory = @"D:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
    })
    .CreateLogger();
```

#### Builder Pattern
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

---

## Release Notes Template

### 🎉 What's New in v{version}

<!-- Highlight major new features -->

### 🐛 Bug Fixes

<!-- List important bug fixes -->

### ⚡ Performance Improvements

<!-- List performance enhancements -->

### 🔧 Other Changes

<!-- List other notable changes -->

### 📦 Installation

```bash
dotnet add package XTinyLog.Serilog --version {version}
```

### 🔗 Links

- [Full Changelog](https://github.com/OsamaAbuSitta/XTinyLog/compare/XTinyLog.Serilog-v{previous_version}...XTinyLog.Serilog-v{version})
- [Documentation](https://github.com/OsamaAbuSitta/XTinyLog/wiki)
- [XTinyLog Core](https://www.nuget.org/packages/XTinyLog/)

---

**Note**: This changelog is automatically updated during releases. For the most up-to-date information, see the [GitHub Releases](https://github.com/OsamaAbuSitta/XTinyLog/releases) page.