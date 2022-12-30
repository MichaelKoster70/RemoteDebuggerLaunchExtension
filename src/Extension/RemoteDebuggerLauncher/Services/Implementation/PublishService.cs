// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Threading;
using static Microsoft.VisualStudio.ProjectSystem.ExportContractNames;

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
      private readonly IStatusbarService statusbar;

      private bool published;

      /// <summary>
      /// Initializes a new instance of the <see cref="PublishService" /> class.
      /// </summary>
      /// <param name="configuredProject">The configuration aggregator.</param>
      /// <param name="session">The session to use.</param>
      /// <param name="logger">The logger service instance to use.</param>
      internal PublishService(ConfiguredProject configuredProject, ILoggerService logger, IStatusbarService statusbar)
      {
         this.configuredProject = configuredProject ?? throw new ArgumentNullException(nameof(configuredProject));
         this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
         this.statusbar = statusbar;
      }

      /// <inheritdoc />
      public async Task StartAsync()
      {
         published = false;

         var outputPath = await configuredProject.GetOutputDirectoryPathAsync().ConfigureAwait(true);
         var projectPath = configuredProject.UnconfiguredProject.FullPath;
         var publishPath = PathHelper.Combine(outputPath, PackageConstants.Publish.OutDir);

         statusbar?.SetText(Resources.PublishStart);
         logger.WriteLine(Resources.PublishStart);

         var startInfo = new ProcessStartInfo("dotnet.exe", $"publish {projectPath} --output {publishPath} -c Debug --no-build --no-self-contained")
         {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput= true,
            RedirectStandardError = true,
         };
         using (var process = Process.Start(startInfo))
         {
            var stdOutput = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(true);
            var stdError = await process.StandardError.ReadToEndAsync().ConfigureAwait(true);
            logger.WriteLine(stdOutput);
            var exitCode = await process.WaitForExitAsync().ConfigureAwait(true);

            if (exitCode > 0)
            {
               logger.WriteLine(stdError);
               logger.WriteLine(Resources.PublishFailed, exitCode);

               throw new RemoteDebuggerLauncherException("publishing failed");
            }

            published = true;
         }

         statusbar?.Clear();
         logger.WriteLine(Resources.PublishSuccess);
      }

      /// <inheritdoc />
      public async Task<string> GetOutputDirectoryPathAsync()
      {
         var outputPath = await configuredProject.GetOutputDirectoryPathAsync().ConfigureAwait(true);

         if (published)
         {
            return PathHelper.Combine(outputPath, PackageConstants.Publish.OutDir);
         }

         return outputPath;
      }
   }
}
