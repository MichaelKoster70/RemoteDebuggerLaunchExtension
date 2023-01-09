// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command Handler to deploy the binaries from the remote device.
   /// </summary>
   /// <remarks>
   /// The remote path to clean is read from the launch settings in all startup projects (launchsettings.json)
   /// </remarks>
   internal sealed class DeployOutputCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0100;

      /// <summary>VS Package that provides this command, not null.</summary>
      private readonly AsyncPackage package;

      /// <summary>
      /// Initializes a new instance of the <see cref="DeployOutputCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private DeployOutputCommand(AsyncPackage package, OleMenuCommandService commandService)
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
      public static DeployOutputCommand Instance
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
         Instance = new DeployOutputCommand(package, commandService);
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
            var dte = await ServiceProvider.GetAutomationModelTopLevelObjectServiceAsync();
            var projectService = await ServiceProvider.GetProjectServiceAsync();
            var optionsPageAccessor = await ServiceProvider.GetOptionsPageServiceAsync();
            var loggerService = await ServiceProvider.GetLoggerServiceAsync();
            var statusbarService = await ServiceProvider.GetStatusbarServiceAsync();
            var waitDialogFactory = await ServiceProvider.GetWaitDialogFactoryAsync(true);

            // do the remaining work on the UI thread
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var lauchProfileAccess = new LaunchProfileAccess(dte, projectService);
            var profiles = await lauchProfileAccess.GetActiveLaunchProfilseWithProjectAsync();

            loggerService.WriteLine(Resources.CommonStartSessionMarker);

            foreach (var profile in profiles)
            {
               var tokenReplacer = profile.ConfiguredProject.UnconfiguredProject.GetDebugTokenReplacerService();

               // get environment variables and msbuild properties resolved
               var resolvedProfile = await tokenReplacer.ReplaceTokensInProfileAsync(profile.LaunchProfile);

               var configurationAggregator = ConfigurationAggregator.Create(profile.LaunchProfile, optionsPageAccessor);
               var remoteOperations = SecureShellRemoteOperations.Create(configurationAggregator, loggerService);
               var publishService = Publish.Create(profile.ConfiguredProject, loggerService, waitDialogFactory);

               remoteOperations.LogHost = true;
               loggerService.WriteLine(Resources.RemoteCommandCommonProjectAndProfile, profile.ConfiguredProject.GetName(), profile.LaunchProfile.Name);

               // Step 1: try to connect to the device
               await remoteOperations.CheckConnectionThrowAsync();

               // Step 2:publish the project if requested
               if (configurationAggregator.QueryPublishOnDeploy())
               {
                  await publishService.StartAsync();
               }

               //// Step 3: Deploy application to target folder
               var outputPath = await publishService.GetOutputDirectoryPathAsync();
               await remoteOperations.DeployRemoteFolderAsync(outputPath, true);
            }
         });
      }
   }
}
