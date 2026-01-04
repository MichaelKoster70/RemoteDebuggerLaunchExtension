// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
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
      private readonly IDebugTokenReplacer tokenReplacer;

      public SecureShellDeployService(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject, IDotnetPublishService publishService, ISecureShellRemoteOperationsService remoteOperations, IDebugTokenReplacer tokenReplacer)
      {
         this.configurationAggregator = configurationAggregator;
         this.configuredProject = configuredProject;
         this.publishService = publishService;
         this.remoteOperations = remoteOperations;
         this.tokenReplacer = tokenReplacer;
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

         // Step 5: deploy additional directories
         await DeployAdditionalDirectoriesAsync(clean);
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
            var remotePath = await tokenReplacer.ReplaceTokensInStringAsync(configurationAggregator.QueryAppFolderPath(), false);
            remotePath = UnixPath.Combine(remotePath, binaryName);

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
            if (!string.IsNullOrEmpty(additionalFilesConfig))
            {
               var appFolderPath = await tokenReplacer.ReplaceTokensInStringAsync(configurationAggregator.QueryAppFolderPath(), false);
               var parser = new AdditionalDeploymentParser(configuredProject.GetProjectFolder(), appFolderPath);
               var additionalFiles = parser.Parse(additionalFilesConfig, true);

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
         catch (ArgumentException ex)
         {
            throw new RemoteDebuggerLauncherException($"Invalid additional files configuration: {ex.Message}", ex);
         }
      }

      /// <summary>
      /// Deploys additional folders specified in the launch profile to the remote device.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      private async Task DeployAdditionalDirectoriesAsync(bool clean)
      {
         try
         {
            var additionalDirectoriesConfig = configurationAggregator.QueryAdditionalDirectories();
            if (!string.IsNullOrEmpty(additionalDirectoriesConfig))
            {
               var parser = new AdditionalDeploymentParser(configuredProject.GetProjectFolder(), configurationAggregator.QueryAppFolderPath());
               var additionalDirectories = parser.Parse(additionalDirectoriesConfig, false);

               // Validate that all source directories exist
               var missingDirectory = additionalDirectories.FirstOrDefault(directoryEntry => !Directory.Exists(directoryEntry.SourcePath));
               if (missingDirectory != null)
               {
                  throw new RemoteDebuggerLauncherException($"Additional directory not found: {missingDirectory.SourcePath}");
               }

               // Deploy all additional directories
               foreach (var directoryEntry in additionalDirectories)
               {
                  await remoteOperations.DeployRemoteFolderAsync(directoryEntry.SourcePath, directoryEntry.TargetPath, clean);
               }
            }
         }
         catch (ArgumentException ex)
         {
            remoteOperations.LogHost = true;
            await remoteOperations.CheckConnectionThrowAsync(false);
            throw new RemoteDebuggerLauncherException($"Invalid additional directories configuration: {ex.Message}", ex);
         }
      }
   }
}
