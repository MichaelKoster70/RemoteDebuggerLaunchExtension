// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using RemoteDebuggerLauncher.SecureShell;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Publish service.
   /// Implements the <see cref="IDotnetPublishService"/> interface.
   /// </summary>
   /// <seealso cref="IDotnetPublishService"/>
   internal class DotnetPublishService : IDotnetPublishService
   {
      private readonly ConfigurationAggregator configurationAggregator;
      private readonly ISecureShellRemoteOperationsService remoteOperations;
      private readonly ConfiguredProject configuredProject;
      private readonly IOutputPaneWriterService outputPaneWriter;
      private readonly IWaitDialogFactoryService waitDialogFactory;

      private bool published;

      /// <summary>
      /// Initializes a new instance of the <see cref="DotnetPublishService" /> class.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator.</param>
      /// <param name="configuredProject">The configured project.</param>
      /// <param name="outputPaneWriter">The logger service instance to use.</param>
      /// <param name="waitDialogFactory">The Wait Dialog Factory service.</param>
      internal DotnetPublishService(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject, ISecureShellRemoteOperationsService remoteOperations, IOutputPaneWriterService outputPaneWriter, IWaitDialogFactoryService waitDialogFactory)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         this.configurationAggregator = configurationAggregator ?? throw new ArgumentNullException(nameof(configurationAggregator));
         this.remoteOperations = remoteOperations ?? throw new ArgumentNullException(nameof(remoteOperations));
         this.configuredProject = configuredProject ?? throw new ArgumentNullException(nameof(configuredProject));
         this.outputPaneWriter = outputPaneWriter ?? throw new ArgumentNullException(nameof(outputPaneWriter));
         this.waitDialogFactory = waitDialogFactory ?? throw new ArgumentNullException(nameof(waitDialogFactory));
      }

      public Task<bool> IsUpToDateAsync() => Task.FromResult(false);

      /// <inheritdoc />
      public async Task StartAsync()
      {
         published = false;

         var projectPath = configuredProject.UnconfiguredProject.FullPath;
         var publishPath = await GetPublishedOutputDirectoryPathAsync();
         var configuration = configuredProject.ProjectConfiguration.Dimensions["Configuration"];

         var publishMode = configurationAggregator.QueryPublishMode();

         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         using (var waitDialog = waitDialogFactory.Create(Resources.PublishWaitDialogCaption, Resources.PublishWaitDialogMessageStart, "", 1, Resources.PublishStart, StatusbarAnimation.Build))
         {
            outputPaneWriter.WriteLine(Resources.PublishStart);

            var arguments = $"publish {projectPath} --output {publishPath} -c {configuration}";

            switch (publishMode)
            {
               case PublishMode.FrameworkDependant:
                  arguments += " --no-build";

                  if (await SupportsFrameworkDependantAsync())
                  {
                     arguments += " --no-self-contained";
                  }
                  break;

               case PublishMode.SelfContained:
                  var runtimeId = await remoteOperations.GetRuntimeIdAsync();
                  arguments += $" --self-contained --runtime {runtimeId}";
                  break;

               default: 
                  break;
            }

            outputPaneWriter.WriteLine(Resources.PublishCommandLine, arguments);

            // Clean the output
            _ = DirectoryHelper.EnsureClean(publishPath);

            // Start publish
            var startInfo = new ProcessStartInfo("dotnet.exe", arguments)
            {
               CreateNoWindow = true,
               UseShellExecute = false,
               RedirectStandardOutput = true,
               RedirectStandardError = true,
            };

            using (var process = Process.Start(startInfo))
            {
#pragma warning disable VSTHRD100 // Avoid async void methods
               async void OnDataReceived(object _, DataReceivedEventArgs e)
               {
                  try
                  {
                     var message = e.Data;

                     await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                     {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                           waitDialog.Update(Resources.PublishWaitDialogMessageUpdate, message, null);
                        }
                        outputPaneWriter.WriteLine(message);
                     });
                  }
                  catch(Exception)
                  { 
                     // Ignore any exception
                  }
               }
#pragma warning restore VSTHRD100 // Avoid async void methods

               process.OutputDataReceived += OnDataReceived;
               process.BeginOutputReadLine();

               var stdError = await process.StandardError.ReadToEndAsync();
               var exitCode = await process.WaitForExitAsync();

               process.OutputDataReceived -= OnDataReceived;

               if (exitCode > 0)
               {
                  outputPaneWriter.WriteLine(stdError);
                  outputPaneWriter.WriteLine(Resources.PublishFailed, exitCode);

                  throw new RemoteDebuggerLauncherException("publishing failed");
               }

               published = true;
            }
         }

         outputPaneWriter.WriteLine(Resources.PublishSuccess);
      }

      /// <inheritdoc />
      public Task<string> GetOutputDirectoryPathAsync()
      {
         if (published)
         {
            return GetPublishedOutputDirectoryPathAsync();
         }
         else
         {
            return configuredProject.GetOutputDirectoryPathAsync();
         }
      }

      private async Task<string> GetPublishedOutputDirectoryPathAsync()
      {
         var baseOutputPath = await configuredProject.GetBaseOutputDirectoryPathAsync();
         var targetFramework = await configuredProject.GetTargetFrameworkAsync();
         var configuration = configuredProject.ProjectConfiguration.Dimensions["Configuration"];

         // Append the deploy/publish base 
         var publishPath = PathHelper.Combine(baseOutputPath, PackageConstants.Publish.OutDir);

         // append the $(Configuration)/$(TargetFramework) dirs
         publishPath = PathHelper.Combine(publishPath, configuration);
         publishPath = PathHelper.Combine(publishPath, targetFramework);

         return publishPath;
      }

      private async Task<bool> SupportsFrameworkDependantAsync()
      {
         // Web Assembly Publish operations require self contained 
         var isWasm = configuredProject.Services.Capabilities.AppliesTo("WebAssembly");
         var projectReferences = await configuredProject.Services.ProjectReferences.GetUnresolvedReferencesAsync();

         foreach (var reference in projectReferences)
         {
            var fullName = reference.EvaluatedIncludeAsFullPath;
            var project = configuredProject.Services.ProjectService.LoadedUnconfiguredProjects.FirstOrDefault(f => f.FullPath.Contains(fullName));
            isWasm |= project != null && project.Services.Capabilities.AppliesTo("WebAssembly");
         }

         // every other project type is fine with framework dependant
         return !isWasm;
      }
   }
}
 