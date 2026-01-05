// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The unconfigured projects package service factory.
   /// Implements <see cref="IUnconfiguredPackageServiceFactory"/>
   /// </summary>
   /// <seealso cref="IUnconfiguredPackageServiceFactory"/>
   [Export(typeof(IUnconfiguredPackageServiceFactory))]
   [Shared(ExportContractNames.Scopes.UnconfiguredProject)]
   internal class UnconfiguredPackageServiceFactory : PackageServiceFactory, IUnconfiguredPackageServiceFactory
   {
      private readonly IDebugTokenReplacer tokenReplacer;

      [ImportingConstructor]
      public UnconfiguredPackageServiceFactory(SVsServiceProvider asyncServiceProvider, IDebugTokenReplacer tokenReplacer, IVsFacadeFactory facadeFactory) :
         base(asyncServiceProvider, facadeFactory, null, tokenReplacer)
      {
         this.tokenReplacer = tokenReplacer;
      }

      [Import]
      internal ILaunchSettingsProvider LaunchSettingsProvider { get; private set; }

      [Import]
      internal Lazy<IActiveConfiguredProjectProvider> ActiveConfiguredProjectProvider { get; private set; }

      /// <inheritdoc />
      public string GetProjectName() => ConfiguredProject.GetProjectName();

      /// <inheritdoc />
      public async Task<bool> ConfigureAsync()
      {
         var activeProfile = LaunchSettingsProvider?.ActiveProfile;
         if (activeProfile == null)
         {
            // shortcut exit if we have no active profile
            return false;
         }

         try
         {
            var resolvedProfile = await tokenReplacer.ReplaceTokensInProfileAsync(activeProfile);
            var configurationAggregator = await CreateConfigurationAggregatorAsync(resolvedProfile);
            var outputPaneWriter = FacadeFactory.GetVsShell().GetOutputPaneWriter();
            var configuredProject = ActiveConfiguredProjectProvider.Value.ActiveConfiguredProject;

            return Configure(configurationAggregator, outputPaneWriter, configuredProject);
         }
         catch (RemoteDebuggerLauncherException)
         {
            //If we end up here, the active profile is not supported => ignore the exception
            return false;
         }
      }
   }
}
