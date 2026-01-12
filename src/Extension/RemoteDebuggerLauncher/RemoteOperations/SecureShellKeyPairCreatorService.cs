// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// SSH key pair creator service.
   /// Implements <see cref="ISecureShellKeyPairCreatorService"/>
   /// </summary>
   internal class SecureShellKeyPairCreatorService : ISecureShellKeyPairCreatorService
   {
      private readonly string defaultKeysFolder;

      public SecureShellKeyPairCreatorService()
      {
         defaultKeysFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), PackageConstants.SecureShell.DefaultKeyPairFolder);
      }

      /// <inheritdoc />
      public string DefaultPrivateKeyPath => GetPrivateKeyPath(SshKeyType.Rsa);
      
      /// <inheritdoc />
      public string DefaultPublicKeyPath => GetPublicKeyPath(SshKeyType.Rsa);

      /// <inheritdoc />
      public async Task<bool> CreateAsync(SshKeyType keyType)
      {
         var privateKeyFile = GetPrivateKeyPath(keyType);
         
         if (!File.Exists(privateKeyFile))
         {
            _ = DirectoryHelper.EnsureExists(defaultKeysFolder);
            string arguments;
            if (keyType == SshKeyType.Rsa)
            {
               arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsRsa, privateKeyFile);
            }
            else if (keyType == SshKeyType.Ecdsa)
            {
               arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsEcdsa, privateKeyFile);
            }
            else
            {
               throw new ArgumentException($"Unsupported key type: {keyType}", nameof(keyType));
            }

            var process = Process.Start(PackageConstants.SecureShell.KeyGenExecutable, arguments);

            return await process.WaitForExitAsync() == 0;
         }

         return true;
      }

      /// <inheritdoc />
      public string GetPrivateKeyPath(SshKeyType keyType)
      {
         var fileName = keyType == SshKeyType.Ecdsa 
            ? PackageConstants.SecureShell.DefaultPrivateKeyFileNameEcdsa 
            : PackageConstants.SecureShell.DefaultPrivateKeyFileName;
         return Path.Combine(defaultKeysFolder, fileName);
      }

      /// <inheritdoc />
      public string GetPublicKeyPath(SshKeyType keyType)
      {
         var fileName = keyType == SshKeyType.Ecdsa 
            ? PackageConstants.SecureShell.DefaultPublicKeyFileNameEcdsa 
            : PackageConstants.SecureShell.DefaultPublicKeyFileName;
         return Path.Combine(defaultKeysFolder, fileName);
      }
   }
}
