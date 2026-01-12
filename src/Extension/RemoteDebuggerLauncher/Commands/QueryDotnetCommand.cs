// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Linq;

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

                     // Execute command -v dotnet on remote device
                     var commandText = PackageConstants.LinuxShellCommands.FormatCommand(PackageConstants.Dotnet.BinaryName);
                     var sessionService = new RemoteOperations.SecureShellSessionService(RemoteOperations.SecureShellSessionSettings.Create(project.Configuration));

                     using (var commandingService = new RemoteOperations.SecureShellSessionCommandingService(sessionService))
                     {
                        var (statusCode, result, error) = await commandingService.TryExecuteCommandAsync(commandText);

                        if (statusCode == 0 && !string.IsNullOrWhiteSpace(result))
                        {
                           // Successfully found dotnet path
                           var dotnetPath = result.Trim();
                           outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandFoundPath, dotnetPath);

                           // Extract the directory path from the full binary path
                           // dotnet binary is typically at /path/to/dotnet, we want /path/to
                           var dotnetInstallPath = System.IO.Path.GetDirectoryName(dotnetPath);
                           if (!string.IsNullOrEmpty(dotnetInstallPath))
                           {
                              // Convert Windows-style path separators to Unix-style if needed
                              dotnetInstallPath = dotnetInstallPath.Replace('\\', '/');
                              
                              // Update the launch profile with the dotnet install path
                              await UpdateLaunchProfileAsync(project, dotnetInstallPath, outputPaneWriter);
                           }
                           else
                           {
                              outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandInvalidPath);
                           }
                        }
                        else
                        {
                           outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandNotFound, error);
                        }
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
      /// Updates the launch profile with the dotnet install folder path by directly modifying the launchSettings.json file.
      /// </summary>
      /// <param name="project">The project factory.</param>
      /// <param name="dotnetInstallPath">The dotnet install path to set.</param>
      /// <param name="outputPaneWriter">The output pane writer for logging.</param>
      private async Task UpdateLaunchProfileAsync(IPackageServiceFactory project, string dotnetInstallPath, IOutputPaneWriterService outputPaneWriter)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         try
         {
            var projectService = await ServiceProvider.GetProjectServiceAsync();
            if (projectService == null)
            {
               outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandCannotUpdateProfile);
               return;
            }

            // Get the unconfigured project to find the launchSettings.json file
            var unconfiguredProjects = projectService.LoadedUnconfiguredProjects;
            foreach (var unconfiguredProject in unconfiguredProjects)
            {
               var factory = unconfiguredProject.Services.ExportProvider.GetService<IUnconfiguredPackageServiceFactory>();
               if (factory != null && factory.LaunchSettingsProvider != null)
               {
                  var activeProfile = factory.LaunchSettingsProvider.ActiveProfile;
                  if (activeProfile != null && activeProfile.Name == project.ActiveLaunchProfileName)
                  {
                     // Find the launchSettings.json file
                     var projectPath = unconfiguredProject.FullPath;
                     var projectDir = Path.GetDirectoryName(projectPath);
                     var propertiesDir = Path.Combine(projectDir, "Properties");
                     var launchSettingsPath = Path.Combine(propertiesDir, "launchSettings.json");

                     if (File.Exists(launchSettingsPath))
                     {
                        // Read the current launchSettings.json
                        var jsonText = await File.ReadAllTextAsync(launchSettingsPath);
                        var jsonObj = JObject.Parse(jsonText);
                        
                        // Update the dotNetInstallFolderPath in the active profile
                        var profiles = jsonObj["profiles"] as JObject;
                        if (profiles != null && profiles[activeProfile.Name] is JObject profileObj)
                        {
                           profileObj["dotNetInstallFolderPath"] = dotnetInstallPath;
                           
                           // Write back to file with indentation
                           var updatedJson = jsonObj.ToString(Newtonsoft.Json.Formatting.Indented);
                           await File.WriteAllTextAsync(launchSettingsPath, updatedJson);
                           
                           outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandSuccess, dotnetInstallPath);
                           return;
                        }
                        else
                        {
                           outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandProfileNotFound);
                           return;
                        }
                     }
                     else
                     {
                        outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandProfileNotFound);
                        return;
                     }
                  }
               }
            }

            outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandProfileNotFound);
         }
         catch (Exception ex)
         {
            outputPaneWriter.WriteLine(Resources.RemoteCommandQueryDotnetCommandUpdateFailed, ex.Message);
         }
      }
   }
}
