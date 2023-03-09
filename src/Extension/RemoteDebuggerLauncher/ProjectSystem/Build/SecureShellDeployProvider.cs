// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Provides the implementation for the deploy phase of build.
   /// Implements <see cref="IDeployProvider"/>
   /// </summary>
   /// <seealso cref=IDeployProvider""/>
   [Export(typeof(IDeployProvider))]
   [Shared(ExportContractNames.Scopes.ConfiguredProject)]
   [AppliesTo(".NET")]
   internal class SecureShellDeployProvider : IDeployProvider
   {
      private readonly IConfiguredPackageServiceFactory packageServiceFactory;

      [ImportingConstructor]
      public SecureShellDeployProvider(IConfiguredPackageServiceFactory packageServiceFactory)
      {
         this.packageServiceFactory = packageServiceFactory;
      }

      [Import]
      internal IProjectThreadingService ProjectThreadingService { get; private set; }

      [Import]
      internal ILaunchSettingsProvider LaunchSettingsProvider { get; private set; }

      public bool IsDeploySupported => LaunchSettingsProvider.ActiveProfile.IsSecureShellRemoteLaunch();

      /// <summary>
      /// Alerts a project that a deployment operation was successful.
      /// </summary>
      /// <remarks>Ignored by this implementation.</remarks>
      public void Commit()
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Signals to start the deploy operation.
      /// </summary>
      /// <param name="cancellationToken">A cancellation token that will be set if the deploy is canceled by the user.</param>
      /// <param name="outputPaneWriter">A TextWriter that will write to the deployment output pane.</param>
      /// <returns> A task that performs the deploy operation.</returns>
      public async Task DeployAsync(CancellationToken cancellationToken, TextWriter outputPaneWriter)
      {
         try
         {
            // configure the services
            await packageServiceFactory.ConfigureAsync(outputPaneWriter);

            var statusbar = await packageServiceFactory.GetStatusbarServiceAsync();
            var deployService = await packageServiceFactory.GetDeployServiceAsync(false);

            // Step 1: Deploy (with publish if configured)
            await deployService.DeployAsync(true, true);

            statusbar.Clear();
         }
         catch (RemoteDebuggerLauncherException ex) 
         {
            await outputPaneWriter.WriteLineAsync(ex.Message);
         }
      }

      /// <summary>
      /// Alerts a deployment project that a deployment operation has failed.
      /// </summary>
      /// <remarks>Ignored by this implementation.</remarks>
      public void Rollback()
      {
         //EMPTY_BODY
      }
   }
}
