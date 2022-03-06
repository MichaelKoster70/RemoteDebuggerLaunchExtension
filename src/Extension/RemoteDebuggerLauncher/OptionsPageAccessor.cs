// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   [Export(typeof(IOptionsPageAccessor))]
   internal class OptionsPageAccessor : IOptionsPageAccessor
   {
      private readonly EnvDTE80.DTE2 dte;

      public OptionsPageAccessor()
      {
         dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
      }

      public string QueryUserName()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageDevice, PackageConstants.OptionsNameUserName);
      }

      public string QueryPrivateKeyFilePath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageDevice, PackageConstants.OptionsNamePrivateKey);
      }

      public AdapterProviderKind QueryAdapterProvider()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetEnumOption<AdapterProviderKind>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageDevice, PackageConstants.OptionsNamePrivateKey);
      }

      public string QueryDotNetInstallPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageLocal, PackageConstants.OptionsNameDotNetInstallPath);
      }

      public string QueryDebuggerInstallPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageLocal, PackageConstants.OptionsNameDebuggerInstallPath);
      }

      public string QueryAppFolderPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageLocal, PackageConstants.OptionsNameAppFolderPath);
      }

      public string QueryPuttyInstallPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetOption<string>(PackageConstants.OptionsCategory, PackageConstants.OptionsPageLocal, PackageConstants.OptionsPuttyInstallPath);
      }

      private T GetOption<T>(string categoryName, string optionsPage, string option) where T : class
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         var properties = dte.get_Properties(categoryName, optionsPage);
         return properties.Item(option).Value as T;
      }

      private T GetEnumOption<T>(string categoryName, string optionsPage, string option) where T : Enum
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         var properties = dte.get_Properties(categoryName, optionsPage);
         return (T)properties.Item(option).Value;
      }
   }
}
