// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Renci.SshNet.Common;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Provides the progress report when copying files using SCP.
   /// This class cannot be inherited.
   /// </summary>
   internal sealed class SecureShellCopyProgressReporter
   {
      private readonly IOutputPaneWriterService outputPaneWriter;
      private long progressBefore; // = 0
      private string filenameBefore = string.Empty;

      internal SecureShellCopyProgressReporter(IOutputPaneWriterService outputPaneWriter)
      {
         this.outputPaneWriter = outputPaneWriter ?? throw new ArgumentNullException(nameof(outputPaneWriter));
      }

      [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Cannot be avoided")]
      public async void OnUploadFile(object _, ScpUploadEventArgs e)
      {
         if (e.Uploaded == e.Size)
         {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            outputPaneWriter.WriteLine(Resources.RemoteCommandUploadOutputPaneDone);
         }
         else
         {
            long progressNow = 100 * e.Uploaded / e.Size;
            if ((progressNow > progressBefore) && (progressNow % 10 == 0))
            {
               progressBefore = progressNow;
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
               outputPaneWriter.Write(Resources.RemoteCommandUploadOutputPaneProgress);
            }
         }

         await TaskScheduler.Default;
      }

      /// <summary>
      /// Event Handler for the <see cref="ScpClient.Uploading"/> event when uploading folders.
      /// </summary>
      /// <param name="_">The sender.</param>
      /// <param name="e">The <see cref="ScpUploadEventArgs"/> instance containing the event data.</param>
      [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Cannot be avoided")]
      public async void OnUploadFolder(object _, ScpUploadEventArgs e)
      {
         if (filenameBefore == e.Filename)
         {
            // same file
            if (e.Uploaded == e.Size)
            {
               await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
               outputPaneWriter.WriteLine(Resources.RemoteCommandUploadOutputPaneDone);
            }
            else
            {
               long progressNow = 100 * e.Uploaded / e.Size;
               if ((progressNow > progressBefore) && (progressNow % 10 == 0))
               {
                  progressBefore = progressNow;
                  await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                  outputPaneWriter.Write(Resources.RemoteCommandUploadOutputPaneProgress);
               }
            }
         }
         else
         {
            // new file
            filenameBefore = e.Filename;
            progressBefore = 0;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            outputPaneWriter.Write(Resources.RemoteCommandUploadOutputPaneStart, e.Filename);

            if (e.Uploaded == e.Size)
            {
               outputPaneWriter.WriteLine(Resources.RemoteCommandUploadOutputPaneDone);
            }
         }

         await TaskScheduler.Default;
      }
   }
}
