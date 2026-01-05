// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class FileLoggerUnitTests
   {
      private string testLogDirectory;
      private string testLogFilePath;

      [TestInitialize]
      public void TestInitialize()
      {
         testLogDirectory = Path.Combine(Path.GetTempPath(), "RemoteDebuggerLauncherTests", Guid.NewGuid().ToString());
         testLogFilePath = Path.Combine(testLogDirectory, "test.log");
      }

      [TestCleanup]
      public void TestCleanup()
      {
         if (Directory.Exists(testLogDirectory))
         {
            try
            {
               Directory.Delete(testLogDirectory, true);
            }
            catch
            {
               // Ignore cleanup errors
            }
         }
      }

      [TestMethod]
      public void TestSerilogLogger_CreatesLogFile()
      {
         // Arrange
         Directory.CreateDirectory(testLogDirectory);
         
         var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(testLogFilePath)
            .CreateLogger();

         var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
         var logger = loggerFactory.CreateLogger("TestCategory");

         // Act
         logger.LogInformation("Test message");
         loggerFactory.Dispose();
         
         // Wait a bit for file to be written
         Thread.Sleep(100);

         // Assert
         Assert.IsTrue(File.Exists(testLogFilePath));
      }

      [TestMethod]
      public void TestSerilogLogger_WritesMessage()
      {
         // Arrange
         Directory.CreateDirectory(testLogDirectory);
         
         var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
               testLogFilePath,
               outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u5}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
               formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();

         var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
         var logger = loggerFactory.CreateLogger("TestCategory");

         // Act
         logger.LogInformation("Test message");
         loggerFactory.Dispose();
         
         // Wait a bit for file to be written
         Thread.Sleep(100);

         // Assert
         var content = File.ReadAllText(testLogFilePath);
         Assert.IsTrue(content.Contains("INFO"));
         Assert.IsTrue(content.Contains("TestCategory"));
         Assert.IsTrue(content.Contains("Test message"));
      }

      [TestMethod]
      public void TestSerilogLogger_DoesNotWriteBelowMinLevel()
      {
         // Arrange
         Directory.CreateDirectory(testLogDirectory);
         
         var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File(testLogFilePath)
            .CreateLogger();

         var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
         var logger = loggerFactory.CreateLogger("TestCategory");

         // Act
         logger.LogInformation("Test message");
         loggerFactory.Dispose();
         
         // Wait a bit
         Thread.Sleep(100);

         // Assert - file should not exist or be empty since Info is below Warning
         if (File.Exists(testLogFilePath))
         {
            var content = File.ReadAllText(testLogFilePath);
            Assert.IsFalse(content.Contains("Test message"));
         }
      }

      [TestMethod]
      public void TestSerilogLogger_WritesException()
      {
         // Arrange
         Directory.CreateDirectory(testLogDirectory);
         
         var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.File(
               testLogFilePath,
               outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u5}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
               formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();

         var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
         var logger = loggerFactory.CreateLogger("TestCategory");
         var exception = new InvalidOperationException("Test exception");

         // Act
         logger.LogError(exception, "Error occurred");
         loggerFactory.Dispose();
         
         // Wait a bit for file to be written
         Thread.Sleep(100);

         // Assert
         var content = File.ReadAllText(testLogFilePath);
         Assert.IsTrue(content.Contains("ERROR"));
         Assert.IsTrue(content.Contains("Error occurred"));
         Assert.IsTrue(content.Contains("InvalidOperationException"));
         Assert.IsTrue(content.Contains("Test exception"));
      }

      [TestMethod]
      public void TestNullLogger_IsEnabled_AlwaysReturnsFalse()
      {
         // Arrange
         var logger = NullLogger.Instance;

         // Act & Assert
         Assert.IsFalse(logger.IsEnabled(LogLevel.Trace));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Debug));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Information));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Warning));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Error));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Critical));
      }

      [TestMethod]
      public void TestNullLogger_Log_DoesNotThrow()
      {
         // Arrange
         var logger = NullLogger.Instance;

         // Act & Assert - should not throw
         logger.LogInformation("Test message");
         logger.LogError(new Exception("Test"), "Error message");
      }
   }
}
