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

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Publish service.
   /// Implements the <see cref="IDotnetPublishService"/> interface.
   /// </summary>
   /// <seealso cref="IDotnetPublishService"/>
   internal class DotnetPublishService : IDotnetPublishService
   {
      private readonly ConfiguredProject configuredProject;
      private readonly IOutputPaneWriterService outputPaneWriter;
      private readonly IWaitDialogFactoryService waitDialogFactory;

      private bool published;

      /// <summary>
      /// Initializes a new instance of the <see cref="DotnetPublishService" /> class.
      /// </summary>
      /// <param name="configuredProject">The configuration aggregator.</param>
      /// <param name="logger">The logger service instance to use.</param>
      /// <param name="waitDialogFactory">The Wait Dialog Factory service.</param>
      internal DotnetPublishService(ConfiguredProject configuredProject, IOutputPaneWriterService outputPaneWriter, IWaitDialogFactoryService waitDialogFactory)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         this.configuredProject = configuredProject ?? throw new ArgumentNullException(nameof(configuredProject));
         this.outputPaneWriter = outputPaneWriter ?? throw new ArgumentNullException(nameof(outputPaneWriter));
         this.waitDialogFactory = waitDialogFactory ?? throw new ArgumentNullException(nameof(waitDialogFactory));
      }

      public Task<bool> IsUpToDateAsync() => Task.FromResult(false);

      /// <inheritdoc />
      public async Task StartAsync()
      {
         published = false;

         var outputPath = await configuredProject.GetOutputDirectoryPathAsync();
         var projectPath = configuredProject.UnconfiguredProject.FullPath;
         var publishPath = await GetPublishedOutputDirectoryPathAsync();
         var configuration = configuredProject.ProjectConfiguration.Dimensions["Configuration"];

         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         using (var waitDialog = waitDialogFactory.Create(Resources.PublishWaitDialogCaption, Resources.PublishWaitDialogMessageStart, "", 1, Resources.PublishStart, StatusbarAnimation.Build))
         {
            outputPaneWriter.WriteLine(Resources.PublishStart);

            var arguments = $"publish {projectPath} --output {publishPath} -c {configuration} --no-build";
            if (await SupportsFrameworkDependantAsync())
            {
               arguments += " --no-self-contained";
            }

            outputPaneWriter.WriteLine(Resources.PublishCommandLine, arguments);

            var startInfo = new ProcessStartInfo("dotnet.exe", arguments)
            {
               CreateNoWindow = true,
               UseShellExecute = false,
               RedirectStandardOutput = true,
               RedirectStandardError = true,
            };

            using (var process = Process.Start(startInfo))
            {
               async void OnDataReceived(object _, DataReceivedEventArgs e)
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

         foreach ( var reference in projectReferences)
         {
            var fullName = reference.EvaluatedIncludeAsFullPath;
            var project = configuredProject.Services.ProjectService.LoadedUnconfiguredProjects.Where(f => f.FullPath.Contains(fullName)).FirstOrDefault();
            isWasm |= project.Services.Capabilities.AppliesTo("WebAssembly");
         }

         // every other project type is fine with framework dependant
         return !isWasm;
      }
   }
}
