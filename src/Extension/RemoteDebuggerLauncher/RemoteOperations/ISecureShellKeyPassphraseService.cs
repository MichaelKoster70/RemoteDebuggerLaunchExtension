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
   internal interface ISecureShellKeyPassphraseService
   {
      /// <summary>
      /// Gets the cached passphrase for the specified private key file.
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      /// <param name="passphrase">The cached passphrase if available</param>
      /// <returns><c>true</c> if passphrase is available, else <c>false</c></returns>
      bool TryGet(string privateKeyFilePath, out string passphrase);

      /// <summary>
      /// Prompts the user for a passphrase and caches it for the current session.
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      /// <param name="passphrase">The cached passphrase if available</param>
      /// <returns><c>true</c> if passphrase is available, else <c>false</c></returns>
      /// <remarks>Must be executed on the Main Thread.</remarks>
      bool Prompt(string privateKeyFilePath, out string passphrase);

      /// <summary>
      /// Clears all cached passphrases.
      /// </summary>
      void Clear();

      /// <summary>
      /// Clears the cached passphrase for the specified private key file.
      /// </summary>
      /// <param name="privateKeyFilePath">The path to the private key file.</param>
      void Clear(string privateKeyFilePath);
   }
}