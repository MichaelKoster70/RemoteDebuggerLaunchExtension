// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   public class SetupModeViewModel : ViewModelBase
   {
      public SetupModeViewModel(SetupMode mode, string displayName)
      {
         Mode = mode;
         DisplayName = displayName;
      }

      public SetupMode Mode { get; }

      public string DisplayName { get; }
   }
}
