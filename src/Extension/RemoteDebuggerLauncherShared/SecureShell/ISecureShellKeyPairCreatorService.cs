// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Interface defining a SSH key pair creator service.
   /// </summary>
   public interface ISecureShellKeyPairCreatorService
   {
      /// <summary>
      /// Gets the absolute path of the default RSA private key.
      /// </summary>
      string DefaultPrivateKeyPath { get; }

      /// <summary>
      /// Gets the absolute path of the default RSA public key.
      /// </summary>
      string DefaultPublicKeyPath { get; }

      /// <summary>
      /// Create a new RSA key pair, if there is no key pair available with the default name.
      /// </summary>
      /// <returns><c>true</c> if successful or the file exists; else <c>false</c></returns>
      Task<bool> CreateAsync();
   }
}
