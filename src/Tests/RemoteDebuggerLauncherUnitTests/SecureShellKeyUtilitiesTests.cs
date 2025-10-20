// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.RemoteOperations;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class SecureShellKeyUtilitiesTests
   {
      [TestMethod]
      public void IsPrivateKeyEncrypted_UnencryptedPemKey_ReturnsFalse()
      {
         // Arrange
         var unencryptedKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEA1X2X3X4X5X6X7X8X9XAXBXCXDXEXFXGXHXIXJXKXLXMXNXOX
...
-----END RSA PRIVATE KEY-----";
         var tempFile = CreateTempKeyFile(unencryptedKey);

         try
         {
            // Act
            var result = SecureShellKeyUtilities.IsPrivateKeyEncrypted(tempFile);

            // Assert
            Assert.IsFalse(result);
         }
         finally
         {
            File.Delete(tempFile);
         }
      }

      [TestMethod]
      public void IsPrivateKeyEncrypted_EncryptedPemKey_ReturnsTrue()
      {
         // Arrange
         var encryptedKey = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: AES-128-CBC,B85DF97E99A8E55E9E1D2D6F8B1B2C4A

U2FsdGVkX1/s7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g7g
...
-----END RSA PRIVATE KEY-----";
         var tempFile = CreateTempKeyFile(encryptedKey);

         try
         {
            // Act
            var result = SecureShellKeyUtilities.IsPrivateKeyEncrypted(tempFile);

            // Assert
            Assert.IsTrue(result);
         }
         finally
         {
            File.Delete(tempFile);
         }
      }

      [TestMethod]
      public void IsPrivateKeyEncrypted_EncryptedOpenSshKey_ReturnsTrue()
      {
         // Arrange
         var encryptedKey = @"-----BEGIN OPENSSH PRIVATE KEY-----
b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBf
...
-----END OPENSSH PRIVATE KEY-----";
         var tempFile = CreateTempKeyFile(encryptedKey);

         try
         {
            // Act
            var result = SecureShellKeyUtilities.IsPrivateKeyEncrypted(tempFile);

            // Assert
            Assert.IsTrue(result);
         }
         finally
         {
            File.Delete(tempFile);
         }
      }

      [TestMethod]
      public void IsPrivateKeyEncrypted_NonexistentFile_ReturnsFalse()
      {
         // Act
         var result = SecureShellKeyUtilities.IsPrivateKeyEncrypted("nonexistent_file.key");

         // Assert
         Assert.IsFalse(result);
      }

      [TestMethod]
      public void IsPrivateKeyEncrypted_UnencryptedOpenSshKey_ReturnsFalse()
      {
         // Arrange - using actual OpenSSH format with "none" cipher and kdf
         var unencryptedKey = @"-----BEGIN OPENSSH PRIVATE KEY-----
b3BlbnNzaC1rZXktdjEAAAAABG5vbmUAAAAEbm9uZQAAAAAAAAABAAABFwAAAAdzc2gtcn
NhAAAAAwEAAQAAAQEA3l7YJJufSZpJXzEwSi4pd2vRyiVseWz5mPnbrtOmct6G5k/em55J
...
-----END OPENSSH PRIVATE KEY-----";
         var tempFile = CreateTempKeyFile(unencryptedKey);

         try
         {
            // Act
            var result = SecureShellKeyUtilities.IsPrivateKeyEncrypted(tempFile);

            // Assert
            Assert.IsFalse(result);
         }
         finally
         {
            File.Delete(tempFile);
         }
      }

      private static string CreateTempKeyFile(string content)
      {
         var tempFile = Path.GetTempFileName();
         File.WriteAllText(tempFile, content, Encoding.UTF8);
         return tempFile;
      }
   }
}