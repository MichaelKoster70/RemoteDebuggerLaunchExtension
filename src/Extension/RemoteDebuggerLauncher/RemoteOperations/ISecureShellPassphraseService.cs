// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Interface defining a service for managing SSH private key passphrases.
   /// </summary>
   internal interface ISecureShellPassphraseService
   {
      /// <summary>
      /// Gets the cached passphrase for the specified private key file.
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      /// <returns>The passphrase if cached; otherwise null.</returns>
      string GetCachedPassphrase(string privateKeyFilePath);

      /// <summary>
      /// Prompts the user for a passphrase and caches it for the current session.
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      /// <returns>The passphrase entered by the user, or null if cancelled.</returns>
      Task<string> PromptAndCachePassphraseAsync(string privateKeyFilePath);

      /// <summary>
      /// Clears all cached passphrases.
      /// </summary>
      void ClearCache();
   }
}