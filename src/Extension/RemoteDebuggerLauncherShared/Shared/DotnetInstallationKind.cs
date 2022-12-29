// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.Shared
{
   /// <summary>
   /// The supported kinds of .NET installation supported this extension.
   /// </summary>
   public enum DotnetInstallationKind
   {
      /// <summary>Install the full .NET SDK.</summary>
      Sdk,

      /// <summary>Install the .NET application runtime.</summary>
      RuntimeNet,

      /// <summary>Install the ASP.NET runtime.</summary>
      RuntimeAspNet
   }
}
