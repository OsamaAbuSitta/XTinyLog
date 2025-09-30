using System;
using XTinyLog;

class Program
{
    static void Main()
    {
        // Create a test logger
        var logger = XLog.Instance;
        
        // Log some test messages to see the collapsible functionality
        logger.Info("Short info message");
        logger.Warning("This is a longer warning message that should show the collapsible functionality with a preview in the header and full content when expanded");
        logger.Error("Critical error occurred");
        logger.Debug("Debug information with detailed data that can be collapsed and expanded");
        
        Console.WriteLine("Test HTML file generated with collapsible log entries.");
        Console.WriteLine("Features implemented:");
        Console.WriteLine("- Expandable/collapsible log entry sections");
        Console.WriteLine("- Keyboard shortcuts (Ctrl+E to expand all, Ctrl+Shift+E to collapse all)");
        Console.WriteLine("- Better visual hierarchy with improved typography");
        Console.WriteLine("- Expand/Collapse buttons in the header");
    }
}