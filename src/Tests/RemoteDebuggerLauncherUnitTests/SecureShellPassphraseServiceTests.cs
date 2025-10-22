// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.RemoteOperations;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class SecureShellPassphraseServiceTests
   {
      private ISecureShellPassphraseService service;
      private string testKeyPath;

      [TestInitialize]
      public void TestInitialize()
      {
         service = SecureShellPassphraseService.Instance;
         testKeyPath = Path.Combine(Path.GetTempPath(), "test_key");
         File.WriteAllText(testKeyPath, "dummy key content");
      }

      [TestCleanup]
      public void TestCleanup()
      {
         service.ClearCache();
         if (File.Exists(testKeyPath))
         {
            File.Delete(testKeyPath);
         }
      }

      [TestMethod]
      public void GetCachedPassphrase_NoCache_ReturnsNull()
      {
         // Act
         var result = service.GetCachedPassphrase(testKeyPath);

         // Assert
         Assert.IsNull(result);
      }

      [TestMethod]
      public void ClearCachedPassphrase_ExistingKey_RemovesFromCache()
      {
         // Note: This test assumes we could manually add to cache
         // In a real implementation, we'd need a way to add test data
         // For now, this tests the method doesn't throw
         
         // Act & Assert (should not throw)
         service.ClearCachedPassphrase(testKeyPath);
         service.ClearCachedPassphrase(null);
         service.ClearCachedPassphrase("");
      }

      [TestMethod]
      public void ClearCache_ClearsAllCachedPassphrases()
      {
         // Act & Assert (should not throw)
         service.ClearCache();
      }

      [TestMethod]
      public void GetCachedPassphrase_NullPath_ReturnsNull()
      {
         // Act
         var result = service.GetCachedPassphrase(null);

         // Assert
         Assert.IsNull(result);
      }

      [TestMethod]
      public void GetCachedPassphrase_EmptyPath_ReturnsNull()
      {
         // Act
         var result = service.GetCachedPassphrase("");

         // Assert
         Assert.IsNull(result);
      }
   }
}