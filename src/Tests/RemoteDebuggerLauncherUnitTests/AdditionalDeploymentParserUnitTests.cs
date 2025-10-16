// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RemoteDebuggerLauncher.Infrastructure;

namespace RemoteDebuggerLauncherUnitTests
{
   [TestClass]
   public class AdditionalDeploymentParserUnitTests
   {
      [TestMethod]
      public void TestParseEmptyString()
      {
         var result = AdditionalDeploymentParser.Parse("");
         Assert.IsNotNull(result);
         Assert.AreEqual(0, result.Count);
      }

      [TestMethod]
      public void TestParseNullString()
      {
         var result = AdditionalDeploymentParser.Parse(null);
         Assert.IsNotNull(result);
         Assert.AreEqual(0, result.Count);
      }

      [TestMethod]
      public void TestParseSingleEntry()
      {
         var result = AdditionalDeploymentParser.Parse("config.xml|settings/config.xml");
         Assert.AreEqual(1, result.Count);
         Assert.AreEqual("config.xml", result[0].SourcePath);
         Assert.AreEqual("settings/config.xml", result[0].TargetPath);
      }

      [TestMethod]
      public void TestParseMultipleEntries()
      {
         var result = AdditionalDeploymentParser.Parse("config.xml|settings/config.xml;data/file.txt|backup/file.txt");
         Assert.AreEqual(2, result.Count);
         
         Assert.AreEqual("config.xml", result[0].SourcePath);
         Assert.AreEqual("settings/config.xml", result[0].TargetPath);
         
         Assert.AreEqual("data/file.txt", result[1].SourcePath);
         Assert.AreEqual("backup/file.txt", result[1].TargetPath);
      }

      [TestMethod]
      public void TestParseWithSpaces()
      {
         var result = AdditionalDeploymentParser.Parse(" config.xml | settings/config.xml ; data/file.txt | backup/file.txt ");
         Assert.AreEqual(2, result.Count);
         
         Assert.AreEqual("config.xml", result[0].SourcePath);
         Assert.AreEqual("settings/config.xml", result[0].TargetPath);
         
         Assert.AreEqual("data/file.txt", result[1].SourcePath);
         Assert.AreEqual("backup/file.txt", result[1].TargetPath);
      }

      [TestMethod]
      public void TestParseSkipsEmptyEntries()
      {
         var result = AdditionalDeploymentParser.Parse("config.xml|settings/config.xml;;data/file.txt|backup/file.txt");
         Assert.AreEqual(2, result.Count);
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatNoPipe()
      {
         AdditionalDeploymentParser.Parse("config.xml");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatTooManyPipes()
      {
         AdditionalDeploymentParser.Parse("config.xml|target|extra");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatEmptySource()
      {
         AdditionalDeploymentParser.Parse("|target/path");
      }

      [TestMethod]
      [ExpectedException(typeof(ArgumentException))]
      public void TestParseInvalidFormatEmptyTarget()
      {
         AdditionalDeploymentParser.Parse("source/path|");
      }
   }
}