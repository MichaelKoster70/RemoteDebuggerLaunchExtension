// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Publish service.
   /// Implements the <see cref="IPublishService"/> interface.
   /// </summary>
   /// <seealso cref="IPublishService"/>
   internal class PublishService : IPublishService
   {
      private readonly ConfiguredProject configuredProject;
      private readonly ILoggerService logger;
      private readonly IWaitDialogFactoryService waitDialogFactory;

      private bool published;

      /// <summary>
      /// Initializes a new instance of the <see cref="PublishService" /> class.
      /// </summary>
      /// <param name="configuredProject">The configuration aggregator.</param>
      /// <param name="logger">The logger service instance to use.</param>
      /// <param name="waitDialogFactory">The Wait Dialog Factory service.</param>
      internal PublishService(ConfiguredProject configuredProject, ILoggerService logger, IWaitDialogFactoryService waitDialogFactory)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         this.configuredProject = configuredProject ?? throw new ArgumentNullException(nameof(configuredProject));
         this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
         this.waitDialogFactory = waitDialogFactory ?? throw new ArgumentNullException(nameof(waitDialogFactory));
      }

      /// <inheritdoc />
      public async Task StartAsync()
      {
         published = false;

         var outputPath = await configuredProject.GetOutputDirectoryPathAsync();
         var projectPath = configuredProject.UnconfiguredProject.FullPath;
         var publishPath = PathHelper.Combine(outputPath, PackageConstants.Publish.OutDir);
         var configuration = configuredProject.ProjectConfiguration.Dimensions["Configuration"];

         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         using (var waitDialog = waitDialogFactory.Create(Resources.PublishWaitDialogCaption, Resources.PublishWaitDialogMessageStart, "", 1, Resources.PublishStart, StatusbarAnimation.Build))
         {
            logger.WriteLine(Resources.PublishStart);

            var startInfo = new ProcessStartInfo("dotnet.exe", $"publish {projectPath} --output {publishPath} -c {configuration} --no-build --no-self-contained")
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
                     logger.WriteLine(message);
                  });
               }

               process.OutputDataReceived += OnDataReceived;
               process.BeginOutputReadLine();

               var stdError = await process.StandardError.ReadToEndAsync();
               var exitCode = await process.WaitForExitAsync();

               process.OutputDataReceived -= OnDataReceived;

               if (exitCode > 0)
               {
                  logger.WriteLine(stdError);
                  logger.WriteLine(Resources.PublishFailed, exitCode);

                  throw new RemoteDebuggerLauncherException("publishing failed");
               }

               published = true;
            }
         }

         logger.WriteLine(Resources.PublishSuccess);
      }

      /// <inheritdoc />
      public async Task<string> GetOutputDirectoryPathAsync()
      {
         var outputPath = await configuredProject.GetOutputDirectoryPathAsync();

         if (published)
         {
            return PathHelper.Combine(outputPath, PackageConstants.Publish.OutDir);
         }

         return outputPath;
      }
   }
}
