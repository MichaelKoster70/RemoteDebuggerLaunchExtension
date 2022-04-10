// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// ViewModel for the Install Type ComboBox.
   /// </summary>
   /// <seealso cref="RemoteDebuggerLauncher.ViewModelBase" />
   public class InstallationTypeViewModel : ViewModelBase
   {
      public InstallationTypeViewModel(DotnetInstallationKind kind, string displayName)
      {
         Kind = kind;
         DisplayName = displayName;
      }

      public DotnetInstallationKind Kind { get; }
      public string DisplayName { get; }
   }
}
