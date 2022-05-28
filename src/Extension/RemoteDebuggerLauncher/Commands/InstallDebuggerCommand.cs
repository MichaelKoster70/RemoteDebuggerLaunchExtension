// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command handler for the Install VS Code Debugger command.
   /// </summary>
   internal sealed class InstallDebuggerCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0100;

      /// <summary>Package that provides this command, not null.</summary>
      private readonly AsyncPackage package;

      /// <summary>Running task instance to prevent multiple executions.</summary>
      private JoinableTask joinableTask;

      /// <summary>
      /// Initializes a new instance of the <see cref="InstallDebuggerCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private InstallDebuggerCommand(AsyncPackage package, OleMenuCommandService commandService)
      {
         this.package = package ?? throw new ArgumentNullException(nameof(package));
         commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

         var menuCommandID = new CommandID(PackageConstants.Commands.CommandSet, CommandId);
         var menuItem = new MenuCommand(this.Execute, menuCommandID);
         commandService.AddCommand(menuItem);
      }

      /// <summary>
      /// Gets the instance of the command.
      /// </summary>
      public static InstallDebuggerCommand Instance
      {
         get;
         private set;
      }

      /// <summary>
      /// Gets the service provider from the owner package.
      /// </summary>
      private IAsyncServiceProvider ServiceProvider => package;

      /// <summary>
      /// Initializes the singleton instance of the command.
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      public static async Task InitializeAsync(AsyncPackage package)
      {
         // Switch to the main thread - the call to AddCommand in InstallDebugger's constructor requires
         // the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new InstallDebuggerCommand(package, commandService);
      }

      /// <summary>
      /// This function is the callback used to execute the command when the menu item is clicked.
      /// See the constructor to see how the menu item is associated with this function using OleMenuCommandService service and MenuCommand class.
      /// </summary>
      /// <param name="sender">Event sender.</param>
      /// <param name="e">Event args.</param>
      private void Execute(object sender, EventArgs e)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         // check, if the command is already running
         if (joinableTask != null)
         {
            return;
         }

         var viewModel = new InstallDebuggerViewModel(ThreadHelper.JoinableTaskFactory);
         var dialog = new InstallDebuggerDialogWindow()
         {
            DataContext = viewModel
         };

         var result = dialog.ShowDialog();

         if (result.HasValue && result.Value)
         {
            joinableTask = package.JoinableTaskFactory.RunAsync(async () =>
            {
               var statusbarService = await ServiceProvider.GetServiceAsync<SStatusbarService, IStatusbarService>();

               try
               {
                  // get all services we need
                  var dte = await ServiceProvider.GetAutomationModelTopLevelObjectServiceAsync().ConfigureAwait(false);
                  var projectService = await ServiceProvider.GetProjectServiceAsync().ConfigureAwait(false);
                  var optionsPageAccessor = await ServiceProvider.GetServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
                  var loggerService = await ServiceProvider.GetServiceAsync<SLoggerService, ILoggerService>();

                  // do the remaining work on the UI thread
                  await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                  var lauchProfileAccess = new LaunchProfileAccess(dte, projectService);
                  var profiles = await lauchProfileAccess.GetActiveLaunchProfilesAsync();

                  statusbarService.SetText(Resources.RemoteCommandInstallDebuggerStatusbarText);
                  loggerService.WriteLine(Resources.CommonStartSessionMarker);

                  foreach (var profile in profiles)
                  {
                     var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
                     var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService);

                     remoteOperations.LogHost = true;

                     loggerService.WriteLine(Resources.RemoteCommandCommonProfile, profile.Name);

                     var success = viewModel.SelectedInstallationModeOnline;
                     if (success)
                     {
                        success = await remoteOperations.TryInstallVsDbgOnlineAsync(viewModel.SelectedVersion);
                     }

                     if (!success)
                     {
                        await remoteOperations.TryInstallVsDbgOfflineAsync(viewModel.SelectedVersion);
                     }
                  }
               }
               catch(Exception exception)
               {
                  VsShellUtilities.ShowMessageBox(package, exception.Message, Resources.RemoteCommandInstallDebuggerCaption, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
               }
               finally
               {
                  statusbarService.Clear();
                  joinableTask = null;
               }
            });
         } 
      }
   }
}
