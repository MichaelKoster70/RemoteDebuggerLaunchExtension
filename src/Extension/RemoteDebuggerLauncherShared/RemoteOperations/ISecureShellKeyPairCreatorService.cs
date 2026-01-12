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
      /// Create a new SSH key pair of the specified type, if there is no key pair available with the default name.
      /// </summary>
      /// <param name="keyType">The type of SSH key to create (RSA or ECDSA).</param>
      /// <returns><c>true</c> if successful or the file exists; else <c>false</c></returns>
      Task<bool> CreateAsync(SshKeyType keyType);

      /// <summary>
      /// Gets the absolute path of the private key for the specified key type.
      /// </summary>
      /// <param name="keyType">The type of SSH key.</param>
      /// <returns>The absolute path of the private key.</returns>
      string GetPrivateKeyPath(SshKeyType keyType);

      /// <summary>
      /// Gets the absolute path of the public key for the specified key type.
      /// </summary>
      /// <param name="keyType">The type of SSH key.</param>
      /// <returns>The absolute path of the public key.</returns>
      string GetPublicKeyPath(SshKeyType keyType);
   }
}
