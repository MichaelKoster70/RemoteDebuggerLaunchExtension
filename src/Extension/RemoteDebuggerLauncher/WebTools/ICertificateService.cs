// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Interface defining the service responsible for creating self signed x.509 certificates for development purposes.
   /// The leaf certificates will be signed by the CA named "Personal Developer Root"
   /// </summary>
   internal interface ICertificateService
   {
      /// <summary>
      /// Create a leaf certificate for device signed by our internal root CA.
      /// </summary>
      /// <param name="deviceName">The device name.</param>
      /// <returns>The x.509 certificate; <c>null</c> for failure.</returns>
      X509Certificate2 CreateDevelopmentCertificate(string deviceName);

      /// <summary>
      /// Create a leaf certificate for device signed by our internal root CA as a PFX file.
      /// </summary>
      /// <param name="deviceName">The device name.</param>
      /// <param name="password">The optional password to protect the private key.</param>
      /// <returns>The byte array holding the PFX file contents; <c>null</c> for failure.</returns>
      byte[] CreateDevelopmentCertificateFile(string deviceName, string password = null);

      /// <summary>
      /// Ensures that the public root certificate is valid and is stored in the trusted root cert store for the current user.
      /// </summary>
      void EnsureSelfSignedRootPresentAndTrusted();

      /// <summary>
      /// Returns a value whether the root certificate is present and valid.
      /// </summary>
      /// <returns><c>true</c> if present and valid; else <c>false</c>.</returns>
      bool IsSelfSignedRootPresent();
   }
}
