// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   public class InstallationModeViewModel : ViewModelBase
   {
      public InstallationModeViewModel(bool mode, string displayName)
      {
         Mode = mode;
         DisplayName = displayName;
      }

      bool Mode { get; }

      public string DisplayName { get; }
   }
}
