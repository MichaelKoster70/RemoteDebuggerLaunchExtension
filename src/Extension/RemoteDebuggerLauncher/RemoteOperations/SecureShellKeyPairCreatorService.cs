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
      private readonly string defaultPrivateFile;
      private readonly string defaultPublicFile;
      private readonly string defaultKeysFolder;

      public SecureShellKeyPairCreatorService()
      {
         defaultKeysFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), PackageConstants.SecureShell.DefaultKeyPairFolder);
         defaultPrivateFile = Path.Combine(defaultKeysFolder, PackageConstants.SecureShell.DefaultPrivateKeyFileName);
         defaultPublicFile = Path.Combine(defaultKeysFolder, PackageConstants.SecureShell.DefaultPublicKeyFileName);
      }

      /// <inheritdoc />
      public string DefaultPrivateKeyPath => defaultPrivateFile;
      
      /// <inheritdoc />
      public string DefaultPublicKeyPath => defaultPublicFile;

      /// <inheritdoc />
      public async Task<bool> CreateAsync(SshKeyType keyType)
      {
         if (!File.Exists(defaultPrivateFile))
         {
            _ = DirectoryHelper.EnsureExists(defaultKeysFolder);
            string arguments;
            if (keyType == SshKeyType.Rsa)
            {
               arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsRsa, defaultPrivateFile);
            }
            else if (keyType == SshKeyType.Ecdsa)
            {
               arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsEcdsa, defaultPrivateFile);
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
   }
}
