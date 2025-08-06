// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;
using RemoteDebuggerLauncher.SecureShell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The package service factory base class.
   /// </summary>
   internal class PackageServiceFactory : IPackageServiceFactory
   {
      private readonly IAsyncServiceProvider asyncServiceProvider;
      private readonly IVsFacadeFactory facadeFactory;
      private IOptionsPageAccessor optionsPageAccessor;
      private ConfigurationAggregator configurationAggregator;
      private ConfiguredProject configuredProject;
      private IOutputPaneWriterService outputPaneWriter;

      protected PackageServiceFactory(SVsServiceProvider asyncServiceProvider, IVsFacadeFactory facadeFactory, ConfiguredProject configuredProject)
      {
         this.asyncServiceProvider = asyncServiceProvider as IAsyncServiceProvider;
         this.facadeFactory = facadeFactory;
         this.configuredProject = configuredProject;
      }

      /// <inheritdoc />
      public string ActiveLaunchProfileName => configurationAggregator.LaunchProfileName;

      /// <inheritdoc />
      public ConfigurationAggregator Configuration => configurationAggregator;

      /// <inheritdoc />
      public IOutputPaneWriterService OutputPane => outputPaneWriter;

      /// <inheritdoc />
      public async Task<ISecureShellDeployService> GetDeployServiceAsync(bool useWaitDialog)
      {
         var publishService = await GetPublishServiceAsync(useWaitDialog);
         var remoteOperations = await GetSecureShellRemoteOperationsAsync();

         return new SecureShellDeployService(configurationAggregator, configuredProject, publishService, remoteOperations);
      }

      /// <inheritdoc />
      public async Task<IDotnetPublishService> GetPublishServiceAsync(bool useWaitDialog)
      {
         var waitDialog = facadeFactory.GetVsShell().GetWaitDialog(useWaitDialog);
         var remoteOperations = await GetSecureShellRemoteOperationsAsync();

         return new DotnetPublishService(configurationAggregator, configuredProject, remoteOperations, outputPaneWriter, waitDialog);
      }

      /// <inheritdoc />
      public async Task<ISecureShellRemoteOperationsService> GetSecureShellRemoteOperationsAsync()
      {
         var statusbar = await GetStatusbarServiceAsync();
         var settings = SecureShellSessionSettings.Create(configurationAggregator);
         var session = new SecureShellSessionService(settings);
         return new SecureShellRemoteOperationsService(configurationAggregator, session, outputPaneWriter, statusbar);
      }

      /// <inheritdoc />
      public Task<IStatusbarService> GetStatusbarServiceAsync() => Task.FromResult(facadeFactory.GetVsShell().GetStatusbar());

      /// <summary>
      /// Gets the VS Facade facade 
      /// </summary>
      protected IVsFacadeFactory FacadeFactory => facadeFactory;

      /// <summary>
      /// Gets the configured project.
      /// </summary>
      protected ConfiguredProject ConfiguredProject => configuredProject;

      /// <summary>
      /// Configures the base services.
      /// </summary>
      /// <param name="configurationAggregator">The configuration object.</param>
      /// <param name="outputPaneWriter">The VS output pane writer.</param>
      /// <param name="configuredProject">The configured project</param>
      /// <returns></returns>
      protected bool Configure(ConfigurationAggregator configurationAggregator, IOutputPaneWriterService outputPaneWriter, ConfiguredProject configuredProject)
      {
         this.configurationAggregator = configurationAggregator;
         this.outputPaneWriter = outputPaneWriter;
         this.configuredProject = configuredProject;

         return this.configurationAggregator != null && this.outputPaneWriter != null && this.configuredProject != null;
      }

      /// <summary>
      /// Configures the base services.
      /// </summary>
      /// <param name="configurationAggregator">The configuration object.</param>
      /// <param name="outputPaneWriter">The VS output pane writer.</param>
      protected void Configure(ConfigurationAggregator configurationAggregator, IOutputPaneWriterService outputPaneWriter)
      {
         this.configurationAggregator = configurationAggregator;
         this.outputPaneWriter = outputPaneWriter;
      }

      /// <summary>
      /// Creates the configuration object for the given launch profile.
      /// </summary>
      /// <param name="launchProfile">The launch profile</param>
      /// <returns>An <see cref="ConfigurationAggregator"/> instance.</returns>
      /// <exception cref="RemoteDebuggerLauncherException">Thrown if the supplied launch profile is not supported.</exception>
      protected async Task<ConfigurationAggregator> CreateConfigurationAggregatorAsync(ILaunchProfile launchProfile)
      {
         if (launchProfile == null || !launchProfile.IsSecureShellRemoteLaunch())
         {
            throw new RemoteDebuggerLauncherException("the active launch profile is not supported");
         }

         return ConfigurationAggregator.Create(launchProfile, await GetOptionsPageAccessorAsync());
      }

      private async Task<IOptionsPageAccessor> GetOptionsPageAccessorAsync() => optionsPageAccessor = optionsPageAccessor ?? await asyncServiceProvider.GetOptionsPageServiceAsync();
   }
}
