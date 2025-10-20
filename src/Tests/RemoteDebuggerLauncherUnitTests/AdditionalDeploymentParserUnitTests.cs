// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.Infrastructure;


namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   [SuppressMessage("Major Code Smell", "S3431:\"[ExpectedException]\" should not be used")]
   public class AdditionalDeploymentParserUnitTests
   {
      [TestMethod]
      public void TestParseEmptyString()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("");

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(0, result.Count);
      }

      [TestMethod]
      public void TestParseNullString()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse(null);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(0, result.Count);
      }

      [TestMethod]
      public void TestParseSingleEntry()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("config.xml|settings/config.xml");

         // Assert
         Assert.AreEqual(1, result.Count);
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[0].SourcePath);
         Assert.AreEqual("/remote/app/folder/settings/config.xml", result[0].TargetPath);
      }

      [TestMethod]
      public void TestParseMultipleEntries()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("config.xml|settings/config.xml;data/file.txt|backup/file.txt");
         Assert.AreEqual(2, result.Count);

         // Assert
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[0].SourcePath);
         Assert.AreEqual("/remote/app/folder/settings/config.xml", result[0].TargetPath);

         Assert.AreEqual("C:\\local\\project\\path\\data\\file.txt", result[1].SourcePath);
         Assert.AreEqual("/remote/app/folder/backup/file.txt", result[1].TargetPath);
      }

      [TestMethod]
      public void TestParseWithSpaces()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse(" config.xml | settings/config.xml ; data/file.txt | backup/file.txt ");
         Assert.AreEqual(2, result.Count);

         // Assert
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[0].SourcePath);
         Assert.AreEqual("/remote/app/folder/settings/config.xml", result[0].TargetPath);

         Assert.AreEqual("C:\\local\\project\\path\\data\\file.txt", result[1].SourcePath);
         Assert.AreEqual("/remote/app/folder/backup/file.txt", result[1].TargetPath);
      }

      [TestMethod]
      public void TestParseSkipsEmptyEntries()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("config.xml|settings/config.xml;;data/file.txt|backup/file.txt");

         // Assert
         Assert.AreEqual(2, result.Count);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatNoPipe()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         _ = parser.Parse("config.xml");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatTooManyPipes()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         _ = parser.Parse("config.xml|target|extra");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatEmptySource()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         _ = parser.Parse("|target/path");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatEmptyTarget()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         _ = parser.Parse("source/path|");
      }

      [TestMethod]
      public void TestParseRootedSourceFile()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("C:\\test\\file.txt|target/file.txt");

         // Assert
         Assert.AreEqual(1, result.Count);
         Assert.AreEqual("C:\\test\\file.txt", result[0].SourcePath);
         Assert.AreEqual("/remote/app/folder/target/file.txt", result[0].TargetPath);
      }

      [TestMethod]
      public void TestParseMixedRootedAndRelativeSourceFiles()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("C:/test/file.txt|target/file.txt;config.xml|settings/config.xml");

         // Assert
         Assert.AreEqual(2, result.Count);
         Assert.AreEqual("C:/test/file.txt", result[0].SourcePath);
         Assert.AreEqual("/remote/app/folder/target/file.txt", result[0].TargetPath);
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[1].SourcePath);
         Assert.AreEqual("/remote/app/folder/settings/config.xml", result[1].TargetPath);
      }

      [TestMethod]
      public void TestParseRootedTargetFile()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("config.xml|/mkt/config.xml");

         // Assert
         Assert.AreEqual(1, result.Count);
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[0].SourcePath);
         Assert.AreEqual("/mkt/config.xml", result[0].TargetPath);
      }

      [TestMethod]
      public void TestParseMixedRootedAndRelativeTargetFiles()
      {
         // Arrange
         var parser = new AdditionalDeploymentParser("C:\\local\\project\\path", "/remote/app/folder");

         // Act
         var result = parser.Parse("config.xml|/mkt/config.xml;data/file.txt|backup/file.txt");

         // Assert
         Assert.AreEqual(2, result.Count);
         Assert.AreEqual("C:\\local\\project\\path\\config.xml", result[0].SourcePath);
         Assert.AreEqual("/mkt/config.xml", result[0].TargetPath);
         Assert.AreEqual("C:\\local\\project\\path\\data\\file.txt", result[1].SourcePath);
         Assert.AreEqual("/remote/app/folder/backup/file.txt", result[1].TargetPath);
      }
   }
}