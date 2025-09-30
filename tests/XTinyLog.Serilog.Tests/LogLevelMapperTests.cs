using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Unit tests for the LogLevelMapper class.
/// </summary>
public class LogLevelMapperTests
{
    [Theory]
    [InlineData(LogEventLevel.Verbose, "Debug")]
    [InlineData(LogEventLevel.Debug, "Debug")]
    [InlineData(LogEventLevel.Information, "Info")]
    [InlineData(LogEventLevel.Warning, "Warn")]
    [InlineData(LogEventLevel.Error, "Error")]
    [InlineData(LogEventLevel.Fatal, "Error")]
    public void MapLogLevel_ShouldMapCorrectly(LogEventLevel serilogLevel, string expectedMethodName)
    {
        // Act
        var result = LogLevelMapper.MapLogLevel(serilogLevel);

        // Assert
        result.Should().Be(expectedMethodName);
    }

    [Fact]
    public void MapLogLevel_ShouldMapAllSerilogLevels()
    {
        // Arrange
        var allSerilogLevels = (LogEventLevel[])Enum.GetValues(typeof(LogEventLevel));

        // Act & Assert
        foreach (var level in allSerilogLevels)
        {
            var result = LogLevelMapper.MapLogLevel(level);
            result.Should().BeOneOf("Debug", "Info", "Warn", "Error");
        }
    }

    [Fact]
    public void MapLogLevel_ShouldReturnInfoForUnknownLevel()
    {
        // Arrange - Cast an invalid enum value
        var unknownLevel = (LogEventLevel)999;

        // Act
        var result = LogLevelMapper.MapLogLevel(unknownLevel);

        // Assert
        result.Should().Be("Info");
    }

    [Fact]
    public void LogMessage_ShouldCallCorrectXLogMethod()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var xLog = XLog.Create(config => config.LogDirectory = tempDir);
        var testMessage = "Test message";

        // Act & Assert - These should not throw exceptions
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Debug, testMessage);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Information, testMessage);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Warning, testMessage);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Error, testMessage);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Fatal, testMessage);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Verbose, testMessage);
    }

    [Fact]
    public async Task LogMessageAsync_ShouldCallCorrectXLogMethod()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var xLog = XLog.Create(config => config.LogDirectory = tempDir);
        var testMessage = "Test async message";
        var cancellationToken = CancellationToken.None;

        // Act & Assert - These should not throw exceptions
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Debug, testMessage, cancellationToken);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, testMessage, cancellationToken);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Warning, testMessage, cancellationToken);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Error, testMessage, cancellationToken);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Fatal, testMessage, cancellationToken);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Verbose, testMessage, cancellationToken);
    }
}