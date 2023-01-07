// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command Handler to clean the deployed binaries from the remote device.
   /// </summary>
   /// <remarks>
   /// The remote path to clean is read from the launch settings in all startup projects (launchsettings.json)
   /// </remarks>
   internal sealed class CleanOutputCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0102;

      /// <summary>VS Package that provides this command, not null.</summary>
      private readonly AsyncPackage package;

      /// <summary>
      /// Initializes a new instance of the <see cref="CleanOutputCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private CleanOutputCommand(AsyncPackage package, OleMenuCommandService commandService)
      {
         this.package = package ?? throw new ArgumentNullException(nameof(package));
         commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

         var menuCommandID = new CommandID(PackageConstants.Commands.CommandSet, CommandId);
         var menuItem = new MenuCommand(Execute, menuCommandID);
         commandService.AddCommand(menuItem);
      }

      /// <summary>
      /// Gets the instance of the command.
      /// </summary>
      public static CleanOutputCommand Instance
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
         // Switch to the main thread - the call to AddCommand in CleanOutputCommand's constructor requires the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new CleanOutputCommand(package, commandService);
      }

      /// <summary>
      /// This function is the callback used to execute the command when the menu item is clicked.
      /// </summary>
      /// <param name="sender">Event sender.</param>
      /// <param name="e">Event args.</param>
      /// <remarks>
      /// See the constructor to see how the menu item is associated with this function using OleMenuCommandService service and MenuCommand class.
      /// </remarks>
      private void Execute(object sender, EventArgs e)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
         package.JoinableTaskFactory.Run(async () =>
         {
            var statusbarService = await ServiceProvider.GetStatusbarServiceAsync();

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
               // get all services we need
               var dte = await ServiceProvider.GetAutomationModelTopLevelObjectServiceAsync();
               var projectService = await ServiceProvider.GetProjectServiceAsync();
               var optionsPageAccessor = await ServiceProvider.GetOptionsPageServiceAsync();
               var loggerService = await ServiceProvider.GetLoggerServiceAsync();

               // do the remaining work on the UI thread
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

               var lauchProfileAccess = new LaunchProfileAccess(dte, projectService);
               var profiles = await lauchProfileAccess.GetActiveLaunchProfilesAsync();

               loggerService.WriteLine(Resources.CommonStartSessionMarker);

               foreach (var profile in profiles)
               {
                  var configurationAggregator = ConfigurationAggregator.Create(profile, optionsPageAccessor);
                  var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService);
                  remoteOperations.LogHost = true;
                  loggerService.WriteLine(Resources.RemoteCommandCommonProfile, profile.Name);
                  await remoteOperations.CleanRemoteFolderAsync();
               }
            }
            catch(Exception exception)
            {
               ShellUtilities.ShowErrorMessageBox(package, exception.Message);
            }
            finally
            {
               statusbarService.Clear();
            }
#pragma warning restore CA1031 // Do not catch general exception types
         });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
      }
   }
}
