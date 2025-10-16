// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using RemoteDebuggerLauncher.Infrastructure;
using RemoteDebuggerLauncher.RemoteOperations;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The deploy via SSH service implementation.
   /// Implements <see cref="ISecureShellDeployService"/>
   /// </summary>
   internal class SecureShellDeployService : ISecureShellDeployService
   {
      private readonly ConfigurationAggregator configurationAggregator;
      private readonly ConfiguredProject configuredProject;
      private readonly IDotnetPublishService publishService;
      private readonly ISecureShellRemoteOperationsService remoteOperations;

      public SecureShellDeployService(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject, IDotnetPublishService publishService, ISecureShellRemoteOperationsService remoteOperations)
      {
         this.configurationAggregator = configurationAggregator;
         this.configuredProject = configuredProject;
         this.publishService = publishService;
         this.remoteOperations = remoteOperations;
      }

      /// <inheritdoc />
      public async Task DeployAsync(bool checkConnection = true, bool logHost = false)
      {
         remoteOperations.LogHost = logHost;

         // Step 1: try to connect to the device
         if (checkConnection)
         {
            await remoteOperations.CheckConnectionThrowAsync(true);
         }

         // Step 2: run the publishing if requested
         if (configurationAggregator.QueryPublishOnDeploy())
         {
            await publishService.StartAsync();
         }

         // Step 3: Deploy application to target folder
         var outputPath = await publishService.GetOutputDirectoryPathAsync();
         var clean = configurationAggregator.QueryDeployClean();
         await remoteOperations.DeployRemoteFolderAsync(outputPath, clean);

         // Step 4: change file permission if self contained
         if (configurationAggregator.QueryPublishOnDeploy() && configurationAggregator.QueryPublishMode() == Shared.PublishMode.SelfContained)
         {
            var binaryName = await configuredProject.GetAssemblyNameAsync();
            var remotePath = UnixPath.Combine(configurationAggregator.QueryAppFolderPath(), binaryName);

            // change file permission to rwx,r,r
            await remoteOperations.ChangeRemoteFilePermissionAsync(remotePath, "rwxr--r--");
         }

         // Step 5: deploy additional files
         await DeployAdditionalFilesAsync();

         // Step 6: deploy additional folders
         await DeployAdditionalFoldersAsync();
      }

      /// <summary>
      /// Deploys additional files specified in the launch profile to the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      private async Task DeployAdditionalFilesAsync()
      {
         var additionalFilesConfig = configurationAggregator.QueryAdditionalFiles();
         if (string.IsNullOrEmpty(additionalFilesConfig))
         {
            return;
         }

         try
         {
            var additionalFiles = AdditionalDeploymentParser.Parse(additionalFilesConfig);
            var projectPath = await configuredProject.GetProjectDirectoryAsync();
            var appFolderPath = configurationAggregator.QueryAppFolderPath();

            foreach (var fileEntry in additionalFiles)
            {
               var sourceFilePath = Path.GetFullPath(Path.Combine(projectPath, fileEntry.SourcePath));
               
               if (!File.Exists(sourceFilePath))
               {
                  remoteOperations.LogHost = true;
                  await remoteOperations.CheckConnectionThrowAsync(false);
                  throw new RemoteDebuggerLauncherException($"Additional file not found: {sourceFilePath}");
               }

               var remoteFilePath = UnixPath.Combine(appFolderPath, fileEntry.TargetPath);
               await remoteOperations.DeployRemoteFileAsync(sourceFilePath, remoteFilePath);
            }
         }
         catch (System.ArgumentException ex)
         {
            remoteOperations.LogHost = true;
            await remoteOperations.CheckConnectionThrowAsync(false);
            throw new RemoteDebuggerLauncherException($"Invalid additional files configuration: {ex.Message}", ex);
         }
      }

      /// <summary>
      /// Deploys additional folders specified in the launch profile to the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      private async Task DeployAdditionalFoldersAsync()
      {
         var additionalFoldersConfig = configurationAggregator.QueryAdditionalFolders();
         if (string.IsNullOrEmpty(additionalFoldersConfig))
         {
            return;
         }

         try
         {
            var additionalFolders = AdditionalDeploymentParser.Parse(additionalFoldersConfig);
            var projectPath = await configuredProject.GetProjectDirectoryAsync();
            var appFolderPath = configurationAggregator.QueryAppFolderPath();

            foreach (var folderEntry in additionalFolders)
            {
               var sourceFolderPath = Path.GetFullPath(Path.Combine(projectPath, folderEntry.SourcePath));
               
               if (!Directory.Exists(sourceFolderPath))
               {
                  remoteOperations.LogHost = true;
                  await remoteOperations.CheckConnectionThrowAsync(false);
                  throw new RemoteDebuggerLauncherException($"Additional folder not found: {sourceFolderPath}");
               }

               var remoteFolderPath = UnixPath.Combine(appFolderPath, folderEntry.TargetPath);
               await remoteOperations.DeployRemoteFolderToTargetAsync(sourceFolderPath, remoteFolderPath, false);
            }
         }
         catch (System.ArgumentException ex)
         {
            remoteOperations.LogHost = true;
            await remoteOperations.CheckConnectionThrowAsync(false);
            throw new RemoteDebuggerLauncherException($"Invalid additional folders configuration: {ex.Message}", ex);
         }
      }
   }
}
