// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics.Metrics;

namespace RemoteDebuggerLauncher.WebTools
{
   internal class CertificateServices : ICertificateServices
   {
      private const int RsaKeySize = 4096;
      private const string RootCertificateName = "Personal Developer Root";
      private const string RootCertificateCommonName = "CN=" + RootCertificateName;
      private const string RootCertificateFriendlyName = "Personal Root CA for ASP.NET Core developer certificates";

      // OID used for HTTPS certs
      private const string AspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";
      private const string AspNetHttpsOidFriendlyName = "ASP.NET Core HTTPS development certificate";

      private const string ServerAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";
      private const string ServerAuthenticationEnhancedKeyUsageOidFriendlyName = "Server Authentication";

      private const int UserCancelledErrorCode = 1223;

      /// <inheritdoc />
      public X509Certificate2 CreateDevelopmentCertificate(string subject)
      {
         using (var rootCert = LoadOrCreateRootCertificate())
         {
            using (var keyPair = RSA.Create(RsaKeySize))
            {
               var sanBuilder = new SubjectAlternativeNameBuilder();
               sanBuilder.AddDnsName("localhost");
               sanBuilder.AddDnsName(subject);

               // create a CSR
               var subjectName = new X500DistinguishedName(subject);
               var csr = new CertificateRequest(subjectName, keyPair, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
               csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
               csr.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, true));
               csr.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection()
                  { new Oid(ServerAuthenticationEnhancedKeyUsageOid, ServerAuthenticationEnhancedKeyUsageOidFriendlyName) }, true));
               csr.CertificateExtensions.Add(sanBuilder.Build(true));
               csr.CertificateExtensions.Add(new X509Extension(
                  new AsnEncodedData(new Oid(AspNetHttpsOid, AspNetHttpsOidFriendlyName), Encoding.ASCII.GetBytes(AspNetHttpsOidFriendlyName)), false));

               // Create the Cert serial numbert
               byte[] serialNumber = new byte[8];
               using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
               {
                  randomNumberGenerator.GetBytes(serialNumber);
               }

               var cert = csr.Create(rootCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1), serialNumber);
               cert.FriendlyName = AspNetHttpsOidFriendlyName;

               return cert;
            }
         }
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
                  DisposeCertificates(certificates);
               }

               store.Close();
            }
         }
         catch(CryptographicException)
         {
            // trown when the find type is invalid -> treat this as cert not present.
            root = null;
         }

         return root;
      }

      private static X509Certificate2 CreateSelfSignedRootCertificate()
      {
         // Create a RSA keypair
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
               try
               {
                  store.Add(publicCertificate);
               }
               catch (CryptographicException exception) when (exception.HResult == UserCancelledErrorCode)
               {
                  //throw new UserCancelledTrustException();
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
