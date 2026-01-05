// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.Logging;

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
            Directory.Delete(testLogDirectory, true);
         }
      }

      [TestMethod]
      public void TestFileLogger_IsEnabled_WithNone_ReturnsFalse()
      {
         // Arrange
         var logger = new FileLogger("Test", testLogFilePath, LogLevel.None);

         // Act & Assert
         Assert.IsFalse(logger.IsEnabled(LogLevel.Trace));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Debug));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Information));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Warning));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Error));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Critical));
         Assert.IsFalse(logger.IsEnabled(LogLevel.None));
      }

      [TestMethod]
      public void TestFileLogger_IsEnabled_WithInformation_ReturnsCorrectValues()
      {
         // Arrange
         var logger = new FileLogger("Test", testLogFilePath, LogLevel.Information);

         // Act & Assert
         Assert.IsFalse(logger.IsEnabled(LogLevel.Trace));
         Assert.IsFalse(logger.IsEnabled(LogLevel.Debug));
         Assert.IsTrue(logger.IsEnabled(LogLevel.Information));
         Assert.IsTrue(logger.IsEnabled(LogLevel.Warning));
         Assert.IsTrue(logger.IsEnabled(LogLevel.Error));
         Assert.IsTrue(logger.IsEnabled(LogLevel.Critical));
         Assert.IsFalse(logger.IsEnabled(LogLevel.None));
      }

      [TestMethod]
      public void TestFileLogger_Log_CreatesLogFile()
      {
         // Arrange
         var logger = new FileLogger("TestCategory", testLogFilePath, LogLevel.Information);

         // Act
         logger.LogInformation("Test message");

         // Assert
         Assert.IsTrue(File.Exists(testLogFilePath));
      }

      [TestMethod]
      public void TestFileLogger_Log_WritesMessage()
      {
         // Arrange
         var logger = new FileLogger("TestCategory", testLogFilePath, LogLevel.Information);

         // Act
         logger.LogInformation("Test message");

         // Assert
         var content = File.ReadAllText(testLogFilePath);
         Assert.IsTrue(content.Contains("[INFO ]"));
         Assert.IsTrue(content.Contains("TestCategory"));
         Assert.IsTrue(content.Contains("Test message"));
      }

      [TestMethod]
      public void TestFileLogger_Log_DoesNotWriteBelowMinLevel()
      {
         // Arrange
         var logger = new FileLogger("TestCategory", testLogFilePath, LogLevel.Warning);

         // Act
         logger.LogInformation("Test message");

         // Assert
         Assert.IsFalse(File.Exists(testLogFilePath));
      }

      [TestMethod]
      public void TestFileLogger_Log_WritesException()
      {
         // Arrange
         var logger = new FileLogger("TestCategory", testLogFilePath, LogLevel.Error);
         var exception = new InvalidOperationException("Test exception");

         // Act
         logger.LogError(exception, "Error occurred");

         // Assert
         var content = File.ReadAllText(testLogFilePath);
         Assert.IsTrue(content.Contains("[ERROR]"));
         Assert.IsTrue(content.Contains("Error occurred"));
         Assert.IsTrue(content.Contains("Exception:"));
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
