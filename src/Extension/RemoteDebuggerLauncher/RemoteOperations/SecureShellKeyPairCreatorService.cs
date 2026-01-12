// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// SSH key pair creator service.
   /// Implements <see cref="ISecureShellKeyPairCreatorService"/>
   /// </summary>
   [Export(typeof(ISecureShellKeyPairCreatorService))]
   internal class SecureShellKeyPairCreatorService : ISecureShellKeyPairCreatorService
   {
      private readonly ILogger logger;

      private readonly string defaultKeysFolder;

      [ImportingConstructor]
      public SecureShellKeyPairCreatorService(ILoggerFactory loggerFactory)
      {
         logger = loggerFactory.CreateLogger<SecureShellKeyPairCreatorService>();
         defaultKeysFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), PackageConstants.SecureShell.DefaultKeyPairFolder);
      }

      /// <inheritdoc />
      public async Task<bool> CreateAsync(SshKeyType keyType)
      {
         logger.LogTrace("CreateAsync: Begin keyType={KeyType}", keyType);

         var privateKeyFile = GetPrivateKeyPath(keyType);
         
         if (!File.Exists(privateKeyFile))
         {
            _ = DirectoryHelper.EnsureExists(defaultKeysFolder);
            string arguments;
            switch (keyType)
            {
               case SshKeyType.Rsa:
                  arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsRsa, privateKeyFile);
                  break;
               case SshKeyType.Ecdsa:
                  arguments = string.Format(CultureInfo.InvariantCulture, PackageConstants.SecureShell.KeyGenArgumentsEcdsa, privateKeyFile);
                  break;
               default:
                  throw new ArgumentException($"Unsupported key type: {keyType}", nameof(keyType));
            }

            logger.LogDebug("CreateAsync: Starting key generation process: {Executable} {Arguments}", PackageConstants.SecureShell.KeyGenExecutable, arguments);
            var process = Process.Start(PackageConstants.SecureShell.KeyGenExecutable, arguments);
            
            logger.LogTrace("CreateAsync: Waiting for key generation process to exit");
            var exitCode = await process.WaitForExitAsync();

            logger.LogDebug("CreateAsync: End. ExitCode={ExitCode}, returns {Result}", exitCode, exitCode == 0);
            return exitCode == 0;
         }

         logger.LogDebug("CreateAsync: Key file already exists: {PrivateKeyFile} returns {Result}", privateKeyFile, true);
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
