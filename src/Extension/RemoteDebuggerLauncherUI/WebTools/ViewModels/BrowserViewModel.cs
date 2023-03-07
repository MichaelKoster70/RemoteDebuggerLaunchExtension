// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.WebTools
{
   public class BrowserViewModel : ViewModelBase
   {
      private readonly IDisplayNameProvider context;

      public BrowserViewModel(IDisplayNameProvider context)
      {
         this.context = context;
      }

      public string DisplayName => context.DisplayName;

      public T GetContext<T>() where T : class, IDisplayNameProvider
      {
         return context as T;
      }
   }
}
