// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Holds ProjectSystem Rule Exports for the custom launch profile
   /// </summary>
   public static class SecureShellRemoteLaunchRuleExporter
   {
      [ExportPropertyXamlRuleDefinition(
          xamlResourceAssemblyName: Generated.AssemblyVersion.Name + ", Version=" + Generated.AssemblyVersion.Version + ", Culture=Neutral, PublicKeyToken=cdf80caaf8a1ba3e",
          xamlResourceStreamName: "XamlRuleToCode:SecureShellRemoteLaunchProfile.xaml",
          context: PropertyPageContexts.Project)]
      [AppliesTo(PackageConstants.LaunchProfile.AppliesTo)]
      [Order(orderPrecedence: 0)]
#pragma warning disable CS0649 // unused, required for the attribute
      public static int MyLaunchProfileRule;
#pragma warning restore CS0649
   }
}
