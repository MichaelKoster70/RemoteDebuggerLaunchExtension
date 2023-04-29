// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher;
using RemoteDebuggerLauncher.WebTools;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class CertificateServicesUnitTests
   {
      private const string RootCertificateCommonName = "CN=Personal Developer Root";

      [TestMethod]
      public void TestEnsureSelfSignedRootPresentCreatesRoot()
      {
         var service = new CertificateService();
         bool certPresent = false;

         RemoveAllRootCerts();
         RemoveAllPublicRootCerts();

         service.EnsureSelfSignedRootPresentAndTrusted();

         using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadOnly);
            certPresent = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, true).Count == 1;
            store.Close();
         }
         Assert.IsTrue(certPresent);

         using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadOnly);
            certPresent = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, true).Count == 1;
            store.Close();
         }
         Assert.IsTrue(certPresent);
      }

      [TestMethod]
      public void TestEnsureSelfSignedRootPresentCreatesPublicRoot()
      {
         var service = new CertificateService();
         bool certPresent = false;

         RemoveAllRootCerts();

         service.EnsureSelfSignedRootPresentAndTrusted();

         using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadOnly);
            certPresent = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, true).Count == 1;
            store.Close();
         }
         Assert.IsTrue(certPresent);
      }

      [TestMethod]
      [ExpectedException(typeof(RemoteDebuggerLauncherException))]
      public void TestEnsureSelfSignedRootPresentCreatesPublicRootThrows()
      {
         var service = new CertificateService();

         RemoveAllPublicRootCerts();

         service.EnsureSelfSignedRootPresentAndTrusted();
      }

      [TestMethod]
      public void TestIsSelfSignedRootPresentReturnsFalseWhenNoCertsPresent()
      {
         var service = new CertificateService();

         RemoveAllRootCerts();

         var certPresent = service.IsSelfSignedRootPresent();
         Assert.IsFalse(certPresent);
      }

      [TestMethod]
      public void TestIsSelfSignedRootPresentReturnsTrueWhenCertPresent()
      {
         var service = new CertificateService();
         service.EnsureSelfSignedRootPresentAndTrusted();


         var certPresent = service.IsSelfSignedRootPresent();
         Assert.IsTrue(certPresent);
      }

      [TestMethod]
      public void TestCreateDevelopmentCertificate()
      {
         var service = new CertificateService();

         using (var cert = service.CreateDevelopmentCertificate("demo"))
         {
            Assert.IsNotNull(cert);

            Assert.AreEqual(cert.Subject, "CN=demo");
         }
      }

      [TestMethod]
      public void TestCreateDevelopmentCertificateFile()
      {
         var service = new CertificateService();

         var rawData = service.CreateDevelopmentCertificateFile("demo");
         Assert.IsNotNull(rawData);
         Assert.IsTrue(rawData.Length > 0);
      }

      private void RemoveAllRootCerts()
      {
         using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadWrite);
            foreach(var cert in store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, false))
            {
               store.Remove(cert);
            }
            store.Close();
         }
      }

      private void RemoveAllPublicRootCerts()
      {
         using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
         {
            store.Open(OpenFlags.ReadWrite);
            foreach (var cert in store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, RootCertificateCommonName, false))
            {
               store.Remove(cert);
            }
            store.Close();
         }
      }
   }
}
