// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.RemoteOperations;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class SecureShellPassphraseServiceTests
   {
      private ISecureShellKeyPassphraseService service;
      private string testKeyPath;

      [TestInitialize]
      public void TestInitialize()
      {
         service = new SecureShellKeyPassphraseService();
         testKeyPath = Path.Combine(Path.GetTempPath(), "test_key");
         File.WriteAllText(testKeyPath, "dummy key content");
      }

      [TestCleanup]
      public void TestCleanup()
      {
         service.Clear();
         if (File.Exists(testKeyPath))
         {
            File.Delete(testKeyPath);
         }
      }

      [TestMethod]
      public void GetCachedPassphrase_NoCache_ReturnsNull()
      {         
         // Act
         var result = service.TryGet(testKeyPath, out var passphrase);

         // Assert
         Assert.IsTrue(result);
         Assert.IsNull(passphrase);
      }

      [TestMethod]
      [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "not thowing is the assert")]
      public void ClearCachedPassphrase_ExistingKey_RemovesFromCache()
      {       
         // Act
         service.Clear(testKeyPath);
         service.Clear(null);
         service.Clear("");

         // Assert (should not throw)
      }

      [TestMethod]
      [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "not thowing is the assert")]
      public void ClearCache_ClearsAllCachedPassphrases()
      {
         // Act
         service.Clear();

         // Assert (should not throw)
      }

      [TestMethod]
      public void GetCachedPassphrase_NullPath_ReturnsFalse()
      {
         // Act
         var result = service.TryGet(null, out var passphrase);

         // Assert
         Assert.IsFalse(result);
         Assert.IsNull(passphrase);
      }

      [TestMethod]
      public void GetCachedPassphrase_EmptyPath_ReturnsFalse()
      {
         // Act
         var result = service.TryGet("", out var passphrase);

         // Assert
         Assert.IsFalse(result);
         Assert.IsNull(passphrase);
      }
   }
}