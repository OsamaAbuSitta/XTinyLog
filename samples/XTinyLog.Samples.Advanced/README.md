# XTinyLog Advanced Configuration Examples

This console application demonstrates advanced XTinyLog configuration patterns, custom formatting, multiple logger instances, performance optimization techniques, and structured logging approaches.

## Features Demonstrated

### 1. Custom Formatting and File Naming
- Dynamic file naming with timestamps and process IDs
- Structured log messages with contextual information
- Custom application naming and directory organization
- Environment-specific configuration patterns

### 2. Multiple Logger Instances
- Component-specific loggers (Database, API, Security, Business)
- Isolated log directories for different application layers
- Different file size limits and naming conventions per component
- Separation of concerns in logging architecture

### 3. Performance Optimization
- Synchronous vs asynchronous logging performance comparison
- Batch logging techniques for high-throughput scenarios
- Memory usage optimization strategies
- Performance metrics collection and reporting

### 4. Advanced File Management
- File rotation based on size limits
- Custom file naming strategies
- Directory organization patterns
- Log file lifecycle management

### 5. Custom HTML Styling
- Enhanced visual presentation with emojis and icons
- Structured formatting for better readability
- Visual differentiation between log levels
- Rich HTML output for improved log analysis

### 6. Structured Logging Patterns
- Session and user tracking across log entries
- Request correlation with unique identifiers
- Business event tracking with context
- Error tracking with correlation IDs
- Consistent formatting for log parsing and analysis

## Running the Sample

```bash
cd samples/XTinyLog.Samples.Advanced
dotnet run
```

## Expected Output Structure

The application creates organized log directories:

```
%TEMP%/XTinyLogAdvanced/
├── CustomFormatting/
│   └── AdvancedApp/
│       └── app_20241230_143022_12345.html
├── MultipleInstances/
│   ├── Database/
│   │   └── DataAccess/
│   │       └── db_operations_2024-12-30.html
│   ├── API/
│   │   └── WebAPI/
│   │       └── api_requests_2024-12-30.html
│   ├── Security/
│   │   └── Security/
│   │       └── security_audit_2024-12-30.html
│   └── Business/
│       └── BusinessLogic/
│           └── business_events_2024-12-30.html
├── Performance/
│   └── PerformanceTest/
│       └── perf_test_20241230_1430.html
├── FileManagement/
│   └── FileRotation/
│       ├── rotating_log_143045.html
│       ├── rotating_log_143046.html
│       └── rotating_log_143047.html
├── CustomStyling/
│   └── StyledLogs/
│       └── styled_log_2024-12-30.html
└── StructuredLogging/
    └── StructuredLogs/
        └── structured_2024-12-30_14.html
```

## Configuration Patterns

### 1. Custom File Naming
```csharp
config.FileName = () => $"app_{DateTime.Now:yyyyMMdd_HHmmss}_{Environment.ProcessId}";
```

### 2. Component-Specific Configuration
```csharp
var dbLogger = XLog.Create(config =>
{
    config.LogDirectory = Path.Combine(baseDirectory, "Database");
    config.ApplicationName = "DataAccess";
    config.MaxFileSize = 2000;
    config.FileName = () => $"db_operations_{DateTime.Now:yyyy-MM-dd}";
});
```

### 3. Performance-Optimized Settings
```csharp
config.MaxFileSize = 5000; // Larger files for fewer I/O operations
```

### 4. Structured Message Format
```csharp
logger.Info($"[SESSION:{sessionId}] [USER:{userId}] [REQUEST:{requestId}] Operation completed");
```

## Advanced Techniques

### Performance Comparison
The sample includes performance benchmarking between synchronous and asynchronous logging:
- Measures throughput (messages per second)
- Compares resource utilization
- Demonstrates async performance benefits

### File Rotation Strategy
Demonstrates automatic file rotation when size limits are reached:
- Small file sizes for quick rotation demonstration
- Multiple files created automatically
- Timestamp-based file naming for chronological ordering

### Structured Logging Benefits
- **Correlation**: Track related events across different log entries
- **Filtering**: Easy filtering by session, user, request, or error ID
- **Analysis**: Structured format enables automated log analysis
- **Debugging**: Quick identification of related events and context

### HTML Enhancement Features
- **Visual Icons**: Emojis and symbols for quick visual scanning
- **Structured Data**: Formatted metrics and status information
- **Interactive Elements**: Searchable and filterable content
- **Responsive Design**: Works well on different screen sizes

## Key Learning Points

1. **Architecture**: How to organize logging in complex applications
2. **Performance**: Async logging benefits and optimization techniques
3. **Maintenance**: File rotation and lifecycle management
4. **Analysis**: Structured logging for better log analysis
5. **Presentation**: Enhanced HTML output for improved readability
6. **Scalability**: Multiple logger instances for different components
7. **Debugging**: Correlation IDs and contextual information
8. **Monitoring**: Performance metrics and system health indicators

## Best Practices Demonstrated

- **Separation of Concerns**: Different loggers for different purposes
- **Consistent Formatting**: Structured message patterns
- **Performance Awareness**: Async logging for high-throughput scenarios
- **Contextual Information**: Session, user, and request tracking
- **Visual Enhancement**: Icons and formatting for better UX
- **File Organization**: Logical directory structure
- **Error Correlation**: Linking related error events
- **Business Context**: Tracking business events and workflows