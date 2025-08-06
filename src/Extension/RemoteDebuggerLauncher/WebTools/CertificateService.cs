// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Implements the Windows Cert store based x.509 certificates for development purposes.
   /// </summary>
   [Export(typeof(ICertificateService))]
   internal class CertificateService : ICertificateService
   {
      // RSA key size
      private const int RsaKeySize = 4096;

      // the current cert version used by ASP.NET 'dotnet dev-certs
      private const int CurrentCertificateVersion = 2;

      // Localhost DNS and CN
      private const string LocalhostDnsName = "localhost";
      private const string LocalhostDistinguishedName = "CN=" + LocalhostDnsName;

      // CA Name
      private const string RootCertificateName = "Personal Developer Root";
      private const string RootCertificateCommonName = "CN=" + RootCertificateName;
      private const string RootCertificateFriendlyName = "Personal Root CA for ASP.NET Core developer certificates";

      // OID used for HTTPS certs
      private const string AspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";
      private const string AspNetHttpsOidFriendlyName = "ASP.NET Core HTTPS development certificate";

      // OID for server auth
      private const string ServerAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";
      private const string ServerAuthenticationEnhancedKeyUsageOidFriendlyName = "Server Authentication";

      // Hresult when the user cancels trusting a certificate
      private const int UserCanceledErrorCode = unchecked((int)0x800704C7);

      [ImportingConstructor]
      public CertificateService()
      {
         //EMPTY_BODY
      }

      /// <inheritdoc />
      public X509Certificate2 CreateDevelopmentCertificate(string deviceName)
      {
         using (var rootCert = LoadOrCreateRootCertificate())
         {
            var cspParameter = new CspParameters();
            cspParameter = new CspParameters(cspParameter.ProviderType, cspParameter.ProviderName, Guid.NewGuid().ToString());

            using (var keyPair = new RSACryptoServiceProvider(RsaKeySize, cspParameter))
            {
               var sanBuilder = new SubjectAlternativeNameBuilder();
               sanBuilder.AddDnsName(LocalhostDnsName);
               sanBuilder.AddIpAddress(IPAddress.Loopback);
               sanBuilder.AddDnsNameAndIpAddress(deviceName);

               // create a CSR
               var subjectName = new X500DistinguishedName(LocalhostDistinguishedName);
               var csr = new CertificateRequest(subjectName, keyPair, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
               csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
               csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, true));
               csr.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection()
                  { new Oid(ServerAuthenticationEnhancedKeyUsageOid, ServerAuthenticationEnhancedKeyUsageOidFriendlyName) }, true));
               csr.CertificateExtensions.Add(sanBuilder.Build(true));
               csr.CertificateExtensions.Add(new X509Extension(new AsnEncodedData(
                  new Oid(AspNetHttpsOid, AspNetHttpsOidFriendlyName), new byte[] { CurrentCertificateVersion }), false));

               // Create the Cert serial number
               byte[] serialNumber = new byte[9];
               using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
               {
                  randomNumberGenerator.GetBytes(serialNumber);
               };

               // Create the certificate
               var cert = csr.Create(rootCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), serialNumber);
               cert.FriendlyName = AspNetHttpsOidFriendlyName;
               cert.PrivateKey = keyPair;

               return cert;
            }
         }
      }

      /// <inheritdoc />
      public byte[] CreateDevelopmentCertificateFile(string deviceName, string password = null)
      {
         byte[] pfxContents = null;

         using (var certificate = CreateDevelopmentCertificate(deviceName))
         {
            pfxContents = string.IsNullOrEmpty(password) ? certificate?.Export(X509ContentType.Pfx) : certificate?.Export(X509ContentType.Pfx, password);
         }

         return pfxContents;
      }

      /// <inheritdoc />
      public void EnsureSelfSignedRootPresentAndTrusted()
      {
         using (var certificate = LoadOrCreateRootCertificate())
         {
            if (!IsSelfSignedPublicRootCertificateTrusted(certificate))
            {
               TrustSelfSignedPublicRootCertificate(certificate);
            }
         }
      }

      /// <inheritdoc />
      public bool IsSelfSignedRootPresent()
      {
         using (var cert = LoadeSelfSignedRootCertificate())
         {
            return cert != null;
         }
      }

      private static X509Certificate2 LoadOrCreateRootCertificate()
      {
         // try to load an existing cert
         var certificate = LoadeSelfSignedRootCertificate();

         if (certificate == null)
         {
            // if loading the cert failed the first time, have it created
            certificate = CreateSelfSignedRootCertificate();
            certificate = StoreSelfSignedRootCertificate(certificate);
            TrustSelfSignedPublicRootCertificate(certificate);
         }
         else if (!certificate.Verify())
         {
            TrustSelfSignedPublicRootCertificate(certificate);
         }


         return certificate;
      }

      private static X509Certificate2 LoadeSelfSignedRootCertificate()
      {
         X509Certificate2 root = null;

         try
         {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
               store.Open(OpenFlags.ReadOnly);

               var certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, true);

               if (certificates.Count > 0)
               {
                  root = certificates[0];
                  certificates.Remove(root);
               }
               else
               {
                  certificates = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, false);

                  var now = DateTime.UtcNow;
                  foreach (var cert in certificates)
                  {
                     if (cert.NotBefore < now && now < cert.NotAfter)
                     {
                        root = cert;
                        certificates.Remove(root);
                        break;
                     }
                  }
               }

               DisposeCertificates(certificates);

               store.Close();
            }
         }
         catch(CryptographicException)
         {
            // thrown when the find type is invalid -> treat this as cert not present.
            root = null;
         }

         return root;
      }

      private static X509Certificate2 CreateSelfSignedRootCertificate()
      {
         // Create a RSA key pair
         using (var keyPair = RSA.Create(RsaKeySize))
         {
            // create a CSR
            var subjectName = new X500DistinguishedName(RootCertificateCommonName);
            var csr = new CertificateRequest(subjectName, keyPair, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
            csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, false));
            csr.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(csr.PublicKey, false));
            csr.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection 
               { new Oid(ServerAuthenticationEnhancedKeyUsageOid, ServerAuthenticationEnhancedKeyUsageOidFriendlyName) }, false));

            // create a self signed cert
            var cert = csr.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
            cert.FriendlyName = RootCertificateFriendlyName;

            return cert;
         }
      }

      private static X509Certificate2 StoreSelfSignedRootCertificate(X509Certificate2 certificate)
      {
         var export = certificate.Export(X509ContentType.Pkcs12, "");
         certificate.Dispose();
         certificate = new X509Certificate2(export, string.Empty, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable)
         {
            FriendlyName = RootCertificateFriendlyName
         };

         Array.Clear(export, 0, export.Length);

         using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
         }

         return certificate;
      }

      private static bool IsSelfSignedPublicRootCertificateTrusted(X509Certificate2 certificate)
      {
         bool present; 

         using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadOnly);
            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true);
            present = certificates.Count > 0;
            DisposeCertificates(certificates);
            store.Close();
         }

         return present;
      }

      private static void TrustSelfSignedPublicRootCertificate(X509Certificate2 certificate)
      {
         using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadWrite);

            // Exit if the correct cert is already installed
            var certificates  = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, true);
            var count = certificates.Count;
            DisposeCertificates(certificates);
            if (count == 1)
            {
               store.Close();
               return;
            }

            using (var publicCertificate = new X509Certificate2(certificate.Export(X509ContentType.Cert)))
            {
               publicCertificate.FriendlyName= RootCertificateFriendlyName;
               try
               {
#pragma warning disable CA5380 // This is required in order to trust our own root CA
                  store.Add(publicCertificate);
#pragma warning restore CA5380
               }
               catch (CryptographicException e) when (e.HResult == UserCanceledErrorCode)
               {
                  throw new RemoteDebuggerLauncherException(Resources.CertificateServicesTrustCancelledByUser, e);
               }
            }
         }
      }

      private static void DisposeCertificates(X509Certificate2Collection disposables)
      {
         foreach (var disposable in disposables)
         {
            try
            {
               disposable.Dispose();
            }
            catch(Exception)
            {
               //Ignore all failures
            }
         }
      }
   }
}
