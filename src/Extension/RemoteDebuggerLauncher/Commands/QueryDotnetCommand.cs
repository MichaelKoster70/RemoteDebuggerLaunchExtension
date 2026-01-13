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
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command handler to query the dotnet path on the remote device and update the launch profile.
   /// </summary>
   internal sealed class QueryDotnetCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0106;

      /// <summary>VS Package that provides this command, not null.</summary>
      private readonly AsyncPackage package;

      /// <summary>Running task instance to prevent multiple executions.</summary>
      private JoinableTask joinableTask;

      /// <summary>
      /// Initializes a new instance of the <see cref="QueryDotnetCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private QueryDotnetCommand(AsyncPackage package, OleMenuCommandService commandService)
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
      public static QueryDotnetCommand Instance
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
         // Switch to the main thread - the call to AddCommand in QueryDotnetCommand's constructor requires the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new QueryDotnetCommand(package, commandService);
      }

      /// <summary>
      /// This function is the callback used to execute the command when the menu item is clicked.
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

         joinableTask = package.JoinableTaskFactory.RunAsync(async () =>
         {
            var vsFacade = await ServiceProvider.GeVsFacadeFactoryAsync();
            var statusbar = vsFacade.GetVsShell().GetStatusbar();

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
               var outputPaneWriter = vsFacade.GetVsShell().GetOutputPaneWriter();

               // do the remaining work on the UI thread
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

               var projects = await vsFacade.GetVsSolution().GetActiveConfiguredProjectsAsync();

               statusbar.SetText(Resources.RemoteCommandQueryDotnetCommandStatusbarText);
               outputPaneWriter.WriteLine(Resources.CommonStartSessionMarker);

               if (projects.Count > 0)
               {
                  // we have project to process
                  foreach (var project in projects)
                  {
                     var remoteOperations = await project.GetSecureShellRemoteOperationsAsync();
                     remoteOperations.LogHost = true;

                     outputPaneWriter.WriteLine(Resources.RemoteCommandCommonProjectAndProfile, project.GetProjectName(), project.ActiveLaunchProfileName);

                     var dotnetInstallPath = await remoteOperations.TryFindDotNetInstallPathAsync();

                     if (!string.IsNullOrEmpty(dotnetInstallPath))
                     {
                        outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandFoundPath, dotnetInstallPath);

                        // Update the launch profile with the dotnet install path
                        await UpdateLaunchProfileAsync(project, dotnetInstallPath, outputPaneWriter);
                     }
                     else
                     {
                        outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandNotFound);
                     }
                  }
               }
               else
               {
                  // let the user know to select the correct launch profile
                  outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetNoProjects);
               }
            }
            catch (Exception exception)
            {
               ShellUtilities.ShowErrorMessageBox(package, Resources.RemoteCommandQueryDotnetCommandCaption, exception.Message);
            }
            finally
            {
               statusbar.Clear();
               joinableTask = null;
            }
#pragma warning restore CA1031 // Do not catch general exception types
         });
      }

      /// <summary>
      /// Updates the launch profile with the dotnet install folder path using the ILaunchProfileEditor service.
      /// </summary>
      /// <param name="project">The project factory.</param>
      /// <param name="dotnetInstallPath">The dotnet install path to set.</param>
      /// <param name="outputPaneWriter">The output pane writer for logging.</param>
      private static async Task UpdateLaunchProfileAsync(IUnconfiguredPackageServiceFactory project, string dotnetInstallPath, IOutputPaneWriterService outputPaneWriter)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var editor = await project.GetLaunchProfileEditorAsync();

         bool success = await editor.UpdateProfilePropertyAsync("dotNetInstallFolderPath", dotnetInstallPath);
         if (success)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandSuccess, dotnetInstallPath);
         }
         else
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandUpdateFailed, "Failed to update profile");
         }

      }
   }
}
