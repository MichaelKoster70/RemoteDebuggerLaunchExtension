// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using RemoteDebuggerLauncher.SecureShell;

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
         await remoteOperations.DeployRemoteFolderAsync(outputPath, true);

         // Step 4: change file permission if self contained
         if (configurationAggregator.QueryPublishOnDeploy() && configurationAggregator.QueryPublishMode() == Shared.PublishMode.SelfContained)
         {
            var binaryName = await configuredProject.GetAssemblyNameAsync();
            var remotePath = UnixPath.Combine(configurationAggregator.QueryAppFolderPath(), binaryName);

            // change file permission to rwx,r,r
            await remoteOperations.ChangeRemoteFilePermissionAsync(remotePath, "rwxr--r--");
         }
      }
   }
}
