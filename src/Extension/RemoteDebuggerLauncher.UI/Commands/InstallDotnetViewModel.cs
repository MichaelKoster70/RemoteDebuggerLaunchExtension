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

namespace RemoteDebuggerLauncher
{
   public class InstallDotnetViewModel : ViewModelBase
   {
      private InstallationModeViewModel selectedInstallationMode;
      private InstallationTypeViewModel selectedInstallationType;

      public InstallDotnetViewModel(JoinableTaskFactory joinableTaskFactory)
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

         InstallationTypes = new List<InstallationTypeViewModel>()
         {
            new InstallationTypeViewModel (InstallationType.Sdk, Resources.InstallationTypeSdkDisplayName),
            new InstallationTypeViewModel (InstallationType.RuntimeNet, Resources.InstallationTypeRuntimeNetDisplayName),
            new InstallationTypeViewModel (InstallationType.RuntimeAspNet, Resources.InstallationTypeRuntimeAspnetDisplayName)
         };

         Versions = new List<VersionViewModel>()
         {
            new VersionViewModel ("current", Resources.VersionCurrentDisplayName),
            new VersionViewModel ("lts", Resources.VersionLtsDisplayName)
         };

         // set the default values
         SelectedInstallationMode = InstallationModes[0];
         SelectedInstallationType = InstallationTypes[0];
         SelectedVersion = Versions[1].Name;
      }

      public bool Online { get; set; } = true;

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

      public IList<InstallationTypeViewModel> InstallationTypes { get; }

      public InstallationTypeViewModel SelectedInstallationType
      {
         get => selectedInstallationType;
         set
         {
            if (SetProperty(ref selectedInstallationType, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public IList<VersionViewModel> Versions { get; }

      public string SelectedVersion { get; set; }

      //public InstallationTypeViewModel SelectedInstallationType
      //{
      //   get => selectedInstallationType;
      //   set
      //   {
      //      if (SetProperty(ref selectedInstallationType, value))
      //      {
      //         OkCommand.RaiseCanExecuteChanged();
      //      }
      //   }
      //}

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