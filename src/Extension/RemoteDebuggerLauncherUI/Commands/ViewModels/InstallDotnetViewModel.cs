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
            new InstallationTypeViewModel (DotnetInstallationKind.Sdk, Resources.InstallDotnetInstallationTypeSdkDisplayName),
            new InstallationTypeViewModel (DotnetInstallationKind.RuntimeNet, Resources.InstallDotnetInstallationTypeRuntimeNetDisplayName),
            new InstallationTypeViewModel (DotnetInstallationKind.RuntimeAspNet, Resources.InstallDotnetInstallationTypeRuntimeAspnetDisplayName)
         };

         Versions = new List<VersionViewModel>()
         {
            new VersionViewModel (Constants.Dotnet.ChannelSTS, Resources.InstallDotnetVersionLatestStsDisplayName),
            new VersionViewModel (Constants.Dotnet.ChannelLTS, Resources.InstallDotnetVersionLatestLtsDisplayName),
            new VersionViewModel (Constants.Dotnet.Channel70, Resources.InstallDotnetVersion70DisplayName),
            new VersionViewModel (Constants.Dotnet.Channel60, Resources.InstallDotnetVersion60DisplayName)
         };

         // set the default values
         SelectedInstallationMode = InstallationModes[0];
         SelectedInstallationType = InstallationTypes[0];
         SelectedItem = Versions[1];
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

      public DotnetInstallationKind SelectedInstallationKind => SelectedInstallationType.Kind;

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