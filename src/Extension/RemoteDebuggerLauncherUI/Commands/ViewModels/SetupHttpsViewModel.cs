// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   public class SetupHttpsViewModel : ViewModelBase
   {
      private SetupModeViewModel selectedSetupMode;

      public SetupHttpsViewModel(JoinableTaskFactory joinableTaskFactory)
      {
         // wire-up commands
         OkCommand = new DelegateCommand<DialogWindow>(HandleOkCommand, canExecute => Validate(), joinableTaskFactory);
         CancelCommand = new DelegateCommand<DialogWindow>(HandleCancelCommand, null, joinableTaskFactory);

         // initialize properties
         SetupModes = new List<SetupModeViewModel>()
         {
            new SetupModeViewModel (SetupMode.Update, Resources.SetupHttpsModeUpdateDisplayName),
            new SetupModeViewModel (SetupMode.Replace, Resources.SetupHttpsModeReplaceDisplayName),
         };
      }

      public IList<SetupModeViewModel> SetupModes { get; }

      public SetupModeViewModel SelectedSetupMode
      {
         get => selectedSetupMode;
         set
         {
            if (SetProperty(ref selectedSetupMode, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public SetupMode SelectedMode => selectedSetupMode?.Mode ?? SetupMode.Update;

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
         return selectedSetupMode != null;
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