using System;
using XTinyLog;

class Program
{
    static void Main()
    {
        // Create a test logger
        var logger = XLog.Instance;
        
        // Log some test messages
        logger.Info("This is an info message to test the responsive CSS framework");
        logger.Warning("This is a warning message with some longer text to see how it wraps in the responsive layout");
        logger.Error("This is an error message");
        logger.Debug("This is a debug message with detailed information about the system state");
        
        // Log some more messages to test filtering
        logger.Info("Another info message");
        logger.Warning("Another warning with different content");
        logger.Error("Critical error occurred during processing");
        
        Console.WriteLine("Test HTML file generated. Check the log directory for the HTML output.");
        Console.WriteLine("The HTML file should now have:");
        Console.WriteLine("- Responsive CSS Grid and Flexbox layouts");
        Console.WriteLine("- CSS custom properties for theme management");
        Console.WriteLine("- Mobile-first responsive breakpoints");
        Console.WriteLine("- Dark/light theme toggle functionality");
        Console.WriteLine("- Improved typography and spacing");
    }
}