## XTinyLog

XTinyLog is a simple C# library for logging as html file, thread safe, configuration less and easy to use.

XTinyLog dll targeting three main versions net451, netstandard1.3 and netstandard2.0 
which support all .net framework versions greater than 4.5.1 and all .net standard greater than 1.3 [.NET implementation support](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Installation

XTinyLog is distributed as a [NuGet package](https://www.nuget.org/packages/XTinyLog), you can install it from the official NuGet Gallery. Please use the following command to install it using the NuGet Package Manager Console window.

```
PM> Install-Package XTinyLog
```

## Usage
```csharp
using System;
using XTinyLog;

namespace XTinyLog.Test
{
     static void Main(string[] args) {
	  XLog.Log.Info("Log Info");
          XLog.Log.Debug("Log Debug");
	  XLog.Log.Warn("Log Warning");
          XLog.Log.Error("Log Error");
          XLog.Log.Error(new NotImplementedException());
	 }
}
```

## Configuration
You can access log configuration by using XLog.Log.Config which contains :
- Config.LogDirectory 		:  default value "C:\" 
- ApplicationName 		:  default value month date with the following format "MM yyyy"
- MaxFileSize 			:  default value 1,000 KB
- FileName  			:  default value date with the following format "dd MM yyyy.html"
	
- Log file path  
"{Config.LogDirectory}\{Config.ApplicationName}\{fileName}.html"
* LogDirectory, ApplicationName and FileName should be a valid system file name, can't contains "\/:*?"<>|"
	
## Configuration and Instance 
- XTinyLog.XLog is a singleton and factory for log instance, if you have multiple instance or different log configuration , you can use a specific instance as

```csharp	
        XLog specialLogInstance = XLog.Create();
        specialLogInstance.Config.LogDirectory = @"D:" ;
        specialLogInstance.Info("Log file to d drive !!");
```

- Custom Configuration , usefull if you need to custom log file name 

```csharp	
	// Using custom default log 
	XLog customLogInstance = XLog.Create((config) =>
	{
		config.FileName = () => $"log_{DateTime.Now:dd MM yy}";
		config.ApplicationName = Assembly.GetExecutingAssembly().GetName().Name;
		config.LogDirectory = @"D:\new log";
	});

	customLogInstance.Info("Log file to d drive !!");

	// Using default log 
	XLog.Log.Info("Log Info");
	XLog.Log.Debug("Log Debug");
	XLog.Log.Error("Log Error");
	XLog.Log.Error(new NotImplementedException());
	XLog.Log.Warn("Log Warning");
```

## Development and Debug
XTinyLog fails silent, but you can enable debug mode which throwing any exception out by adding conditional compilation symbol "DEBUG_LOG"	
