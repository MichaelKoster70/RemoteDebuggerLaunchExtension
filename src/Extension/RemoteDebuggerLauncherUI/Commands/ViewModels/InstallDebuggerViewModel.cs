// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   public class InstallDebuggerViewModel : ViewModelBase
   {
      private InstallationModeViewModel selectedInstallationMode;

      public InstallDebuggerViewModel(JoinableTaskFactory joinableTaskFactory)
      {
         // wire-up commands
         OkCommand = new DelegateCommand<DialogWindow>(HandleOkCommand, canExecute => Validate(), joinableTaskFactory);
         CancelCommand = new DelegateCommand<DialogWindow>(HandleCancelCommand, null, joinableTaskFactory);

         // initialize properties
         InstallationModes = new List<InstallationModeViewModel>()
         {
            new InstallationModeViewModel (true, Resources.InstallationModeOnlineDisplayName),
            new InstallationModeViewModel (false, Resources.InstallationModeOfflineDisplayName),
         };

         Versions = new List<VersionViewModel>()
         {
            new VersionViewModel (Constants.Debugger.VersionLatest, Resources.InstallDebuggerVersionLatestDisplayName),
            new VersionViewModel (Constants.Debugger.VersionVs2022, Resources.InstallDebuggerVersionVs2022DisplayName)
         };

         // set the default values
         SelectedInstallationMode = InstallationModes[0];
         SelectedItem = Versions[0];
         SelectedText = SelectedItem.DisplayName;
      }

      public IList<InstallationModeViewModel> InstallationModes { get; }

      public InstallationModeViewModel SelectedInstallationMode
      {
         get => selectedInstallationMode;
         set
         {
            if (SetProperty(ref selectedInstallationMode, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public bool SelectedInstallationModeOnline => SelectedInstallationMode.Mode;

      public IList<VersionViewModel> Versions { get; }

      public VersionViewModel SelectedItem { get; set; }

      public string SelectedText { get; set; }

      public string SelectedVersion
      {
         get
         {
            return SelectedItem?.Name ?? SelectedText;
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
         return SelectedInstallationMode != null;
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