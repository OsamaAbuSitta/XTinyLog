# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Modern responsive HTML interface with CSS Grid and Flexbox layouts
- Dark/light theme toggle with system preference detection
- Real-time log filtering and search functionality
- Interactive features: collapsible entries, keyboard shortcuts
- Export functionality (JSON, CSV formats)
- Async logging methods with cancellation token support
- Fluent configuration API with builder pattern
- Comprehensive configuration validation
- Enhanced error handling with internal logging system
- Multi-framework support (.NET Framework 4.6.1, .NET Standard 2.0, .NET 6.0, .NET 8.0)
- Thread-safe concurrent logging operations
- XML documentation for all public APIs
- Comprehensive test suite with unit, integration, and performance tests
- GitHub Actions CI/CD workflows
- Community governance files (CONTRIBUTING.md, CODE_OF_CONDUCT.md, SECURITY.md)
- GitHub issue and PR templates
- Enhanced README with badges, examples, and screenshots

### Changed
- **BREAKING**: Minimum .NET Framework version increased from 4.5.1 to 4.6.1
- **BREAKING**: Configuration validation now throws exceptions for invalid settings
- Improved HTML output with modern CSS and responsive design
- Enhanced package metadata with comprehensive information
- Updated project structure following modern .NET conventions
- Replaced Task.Run().Wait() with proper async/await patterns
- Improved error messages and exception handling

### Fixed
- Thread safety issues in concurrent logging scenarios
- Memory leaks in high-volume logging situations
- File locking issues during log rotation
- HTML escaping for log content to prevent XSS

### Security
- Added input validation and sanitization
- Implemented secure file handling practices
- Added security policy and vulnerability reporting procedures

## [1.0.0] - 2023-XX-XX

### Added
- Initial release of XTinyLog
- Basic HTML logging functionality
- Simple configuration options
- Support for Info, Debug, Warn, and Error log levels
- Thread-safe logging operations
- Configurable log directory and file naming
- File size-based log rotation

### Features
- Singleton and factory pattern for logger instances
- Custom configuration support
- HTML output with basic styling
- Multi-framework targeting (.NET Framework 4.5.1, .NET Standard 1.3, .NET Standard 2.0)

---

## Migration Guides

### Migrating from 1.x to 2.x

#### Breaking Changes

1. **Minimum Framework Version**
   ```diff
   - .NET Framework 4.5.1
   + .NET Framework 4.6.1
   ```

2. **Configuration Validation**
   ```csharp
   // Old: Silent failures
   var logger = XLog.Create(config =>
   {
       config.LogDirectory = "invalid|path"; // Would fail silently
   });
   
   // New: Throws InvalidOperationException
   var logger = XLog.Create(config =>
   {
       config.LogDirectory = "invalid|path"; // Throws exception
   });
   ```

3. **Async Methods**
   ```csharp
   // New async methods available
   await XLog.Log.InfoAsync("Message");
   await XLog.Log.ErrorAsync("Error message");
   ```

#### New Features You Can Use

1. **Fluent Configuration API**
   ```csharp
   var logger = XLog.Create(builder => builder
       .SetLogDirectory(@"C:\Logs")
       .SetApplicationName("MyApp")
       .SetMaxFileSize(10 * 1024 * 1024)
   );
   ```

2. **Enhanced HTML Interface**
   - Responsive design works on mobile devices
   - Dark/light theme toggle
   - Real-time search and filtering
   - Export capabilities

3. **Better Error Handling**
   ```csharp
   try
   {
       var logger = XLog.Create(config =>
       {
           config.LogDirectory = invalidPath;
       });
   }
   catch (InvalidOperationException ex)
   {
       // Handle configuration errors
       Console.WriteLine($"Configuration error: {ex.Message}");
   }
   ```

#### Recommended Upgrade Steps

1. **Update NuGet Package**
   ```bash
   dotnet add package XTinyLog --version 2.0.0
   ```

2. **Update Target Framework** (if using .NET Framework 4.5.1)
   ```xml
   <TargetFramework>net461</TargetFramework>
   ```

3. **Add Error Handling** for configuration
   ```csharp
   try
   {
       var logger = XLog.Create(/* your config */);
   }
   catch (InvalidOperationException ex)
   {
       // Handle invalid configuration
   }
   ```

4. **Consider Using Async Methods** for better performance
   ```csharp
   // Replace synchronous calls
   XLog.Log.Info("Message");
   
   // With asynchronous calls
   await XLog.Log.InfoAsync("Message");
   ```

5. **Test Your Application** thoroughly, especially:
   - Configuration validation
   - File path handling
   - Concurrent logging scenarios

---

## Release Notes Template

<!-- This template is used for automated releases -->

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
dotnet add package XTinyLog --version {version}
```

### 🔗 Links

- [Full Changelog](https://github.com/OsamaAbuSitta/XTinyLog/compare/v{previous_version}...v{version})
- [Documentation](https://github.com/OsamaAbuSitta/XTinyLog/wiki)
- [Migration Guide](#migration-guides)

### 🙏 Contributors

<!-- List contributors for this release -->

---

**Note**: This changelog is automatically updated during releases. For the most up-to-date information, see the [GitHub Releases](https://github.com/OsamaAbuSitta/XTinyLog/releases) page.