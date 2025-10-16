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
         var clean = configurationAggregator.QueryDeployClean();
         await DeployApplicationBinariesAsync(clean);

         // Step 4: deploy additional files
         await DeployAdditionalFilesAsync();

         // Step 5: deploy additional folders
         await DeployAdditionalFoldersAsync(clean);
      }

      private async Task DeployApplicationBinariesAsync(bool clean)
      {
         // Deploy application to target folder
         var outputPath = await publishService.GetOutputDirectoryPathAsync();
         await remoteOperations.DeployRemoteFolderAsync(outputPath, clean);

         // If self-contained deployment, set the execute permission on the main binary
         if (configurationAggregator.QueryPublishOnDeploy() && configurationAggregator.QueryPublishMode() == Shared.PublishMode.SelfContained)
         {
            var binaryName = await configuredProject.GetAssemblyNameAsync();
            var remotePath = UnixPath.Combine(configurationAggregator.QueryAppFolderPath(), binaryName);

            // change file permission to rwx,r,r
            await remoteOperations.ChangeRemoteFilePermissionAsync(remotePath, "rwxr--r--");
         }
      }

      /// <summary>
      /// Deploys additional files specified in the launch profile to the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      private async Task DeployAdditionalFilesAsync()
      {
         try
         {
            var additionalFilesConfig = configurationAggregator.QueryAdditionalFiles();
            if (string.IsNullOrEmpty(additionalFilesConfig))
            {
               var parser = new AdditionalDeploymentParser(configuredProject.GetProjectFolder(), configurationAggregator.QueryAppFolderPath());
               var additionalFiles = parser.Parse(additionalFilesConfig);

               foreach (var fileEntry in additionalFiles)
               {
                  if (!File.Exists(fileEntry.SourcePath))
                  {
                     remoteOperations.LogHost = true;
                     await remoteOperations.CheckConnectionThrowAsync(false);
                     throw new RemoteDebuggerLauncherException($"Additional file not found: {fileEntry.SourcePath}");
                  }

                  await remoteOperations.DeployRemoteFileAsync(fileEntry.SourcePath, fileEntry.TargetPath);
               }
            }
         }
         catch (System.ArgumentException ex)
         {
            throw new RemoteDebuggerLauncherException($"Invalid additional files configuration: {ex.Message}", ex);
         }
      }

      /// <summary>
      /// Deploys additional folders specified in the launch profile to the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      private async Task DeployAdditionalFoldersAsync(bool clean)
      {
         try
         {
            var additionalFoldersConfig = configurationAggregator.QueryAdditionalFolders();
            if (!string.IsNullOrEmpty(additionalFoldersConfig))
            {
               var parser = new AdditionalDeploymentParser(configuredProject.GetProjectFolder(), configurationAggregator.QueryAppFolderPath());
               var additionalFolders = parser.Parse(additionalFoldersConfig);

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

               // Validate that all source folders exist
               foreach (var folderEntry in additionalFolders)
               {
                  if (!Directory.Exists(folderEntry.SourcePath))
                  {
                     throw new RemoteDebuggerLauncherException($"Additional folder not found: {folderEntry.SourcePath}");
                  }
               }
#pragma warning restore S3267

               // Deploy all additional folders
               foreach (var folderEntry in additionalFolders)
               {
                  await remoteOperations.DeployRemoteFolderToTargetAsync(folderEntry.SourcePath, folderEntry.TargetPath, clean);
               }
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
