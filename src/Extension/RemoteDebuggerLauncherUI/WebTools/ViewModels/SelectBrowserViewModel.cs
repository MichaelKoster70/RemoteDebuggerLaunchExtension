// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher.WebTools
{
   public class SelectBrowserViewModel : ViewModelBase
   {
      private BrowserViewModel selectedBrowser;

      public SelectBrowserViewModel(JoinableTaskFactory joinableTaskFactory)
      {
         // wire-up commands
         OkCommand = new DelegateCommand<DialogWindow>(HandleOkCommand, canExecute => Validate(), joinableTaskFactory);
         CancelCommand = new DelegateCommand<DialogWindow>(HandleCancelCommand, null, joinableTaskFactory);

         // initialize properties
         Browsers = new List<BrowserViewModel>();
      }

      public IList<BrowserViewModel> Browsers { get; }

      public BrowserViewModel SelectedBrowser
      {
         get => selectedBrowser;
         set
         {
            if (SetProperty(ref selectedBrowser, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public DelegateCommand<DialogWindow> OkCommand { get; }

      public DelegateCommand<DialogWindow> CancelCommand { get; }

      private void HandleOkCommand(DialogWindow dialog)
      {
         if (dialog != null)
         {
            dialog.DialogResult = true;
            dialog.Close();
         }
      }

      private bool Validate()
      {
         return selectedBrowser != null;
      }

      private void HandleCancelCommand(DialogWindow dialog)
      {
         if (dialog != null)
         {
            dialog.DialogResult = false;
            dialog.Close();
         }
      }
   }
}