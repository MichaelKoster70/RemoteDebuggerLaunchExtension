// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   public class InstallationTypeViewModel : ViewModelBase
   {
      public InstallationTypeViewModel(InstallationType type, string displayName)
      {
         Type = type;
         DisplayName = displayName;
      }

      InstallationType Type { get;  }
      public string DisplayName { get; }
   }
}
