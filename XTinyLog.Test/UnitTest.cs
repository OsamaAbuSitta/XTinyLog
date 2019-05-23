using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace XTinyLog.Test
{

    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void Basic_Log_Test()
        {

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
        }

    }
}