// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   public class VersionViewModel : ViewModelBase
   {
      public VersionViewModel(string name, string displayName)
      {
         Name = name;
         DisplayName = displayName;
      }

      public string Name { get; }

      public string DisplayName { get; }
   }
}
