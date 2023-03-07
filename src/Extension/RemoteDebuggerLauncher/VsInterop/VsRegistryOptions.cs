// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace RemoteDebuggerLauncher.WebTools
{
   [Export(typeof(IVsRegistryOptions))]
   internal class VsRegistryOptions : IVsRegistryOptions
   {
      private const string WebProjectsRegKey = "\\WebProjects";
      private const string EnableDebugTargetsObserver = "EnableDebugTargetsObserver";

      private readonly IVsFacadeFactory vsFacadeFactory;
      private string registryRoot;

      [ImportingConstructor]
      public VsRegistryOptions(IVsFacadeFactory vsFacadeFactory)
      {
         this.vsFacadeFactory = vsFacadeFactory;
      }

      private void Initialize()
      {
         if (registryRoot != null)
         {
            return;
         }

#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
         ThreadHelper.JoinableTaskFactory.Run(async () =>
         {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            registryRoot = vsFacadeFactory.GetVsShell().GetVSRegistryRoot();
         });
#pragma warning restore VSTHRD102
      }

      public bool WebToolsEnableDebugTargetsObserver => GetBoolValue(WebProjectsRegKey, EnableDebugTargetsObserver, true);

      private bool GetBoolValue(string subkey, string option, bool defaultIfNotSet = false)
      {
         Initialize();

         using (var registryKey = Registry.CurrentUser.OpenSubKey(registryRoot + subkey))
         {
            if (registryKey.GetValue(option, 0) is int keyValue)
            {
               return keyValue == 1;
            }
         }

         return defaultIfNotSet;
      }
   }
}