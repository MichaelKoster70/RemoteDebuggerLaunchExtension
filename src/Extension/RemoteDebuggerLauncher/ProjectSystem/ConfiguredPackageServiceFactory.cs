// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The configured projects package service factory.
   /// Implements <see cref=IConfiguredPackageServiceFactory""/>
   /// </summary>
   [Export(typeof(IConfiguredPackageServiceFactory))]
   [Shared(ExportContractNames.Scopes.ConfiguredProject)]
   internal sealed class ConfiguredPackageServiceFactory : PackageServiceFactory, IConfiguredPackageServiceFactory
   {
      private readonly IDebugTokenReplacer tokenReplacer;

      [ImportingConstructor]
      public ConfiguredPackageServiceFactory(SVsServiceProvider asyncServiceProvider, IVsFacadeFactory facadeFactory, IDebugTokenReplacer tokenReplacer, ConfiguredProject configuredProject) :
         base (asyncServiceProvider, facadeFactory, configuredProject)
      {
         this.tokenReplacer = tokenReplacer;
      }

      [Import]
      internal ILaunchSettingsProvider LaunchSettingsProvider { get; private set; }

      /// <inheritdoc />
      public async Task ConfigureAsync(ILaunchProfile launchProfile)
      {
         var resolvedProfile = await tokenReplacer.ReplaceTokensInProfileAsync(launchProfile);
         var configurationAggregator = await CreateConfigurationAggregatorAsync(resolvedProfile);
         var outputPaneWriter = FacadeFactory.GetVsShell().GetOutputPaneWriter();
         Configure(configurationAggregator, outputPaneWriter);
      }

      /// <inheritdoc />
      public async Task ConfigureAsync(TextWriter textWriter)
      {
         var resolvedProfile = await tokenReplacer.ReplaceTokensInProfileAsync(LaunchSettingsProvider?.ActiveProfile);
         var configurationAggregator = await CreateConfigurationAggregatorAsync(resolvedProfile);
         var outputPaneWriter = new OutputPaneTextWriterService(textWriter);
         Configure(configurationAggregator, outputPaneWriter);
      }
   }
}
