// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Interface defining Visual Studio registry options needed by the extension.
   /// </summary>
   internal interface IVsRegistryOptions
   {
      /// <summary>
      /// Gets the 'WebToolsEnableDebugTargetsObserver' registry value
      /// </summary>
      /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
      bool WebToolsEnableDebugTargetsObserver { get; }
   }
}
