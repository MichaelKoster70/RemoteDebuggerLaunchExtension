// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Async wrapper for <see cref="IOutputPaneWriterService"/> making sure calls are marshaled to the UI thread.
   /// </summary>
   internal class OutputPaneWriterServiceAsync
   {
      private readonly IOutputPaneWriterService outputPaneWriter;

      /// <summary>
      /// Initializes a new instance of the <see cref="LoggerService"/> class.
      /// </summary>
      public OutputPaneWriterServiceAsync(IOutputPaneWriterService outputPaneWriter)
      {
         this.outputPaneWriter = outputPaneWriter;
      }

      public static OutputPaneWriterServiceAsync Create(IOutputPaneWriterService outputPaneWriter)
      {
         return new OutputPaneWriterServiceAsync(outputPaneWriter);
      }

      public async Task WriteAsync(string message, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(message, activate);
      }

      public async Task WriteAsync(string message, object arg0, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(string.Format(message, arg0), activate);
      }

      public async Task WriteAsync(string message, object arg0, object arg1, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(message, arg0, arg1, activate);
      }
      public async Task WriteAsync(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(message, arg0, arg1, arg2, activate);
      }

      public async Task WriteAsync(bool predicate, string message, object arg0, object arg1, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(predicate, message, arg0, arg1, activate);
      }

      public async Task WriteAsync(bool predicate, string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.Write(predicate, message, arg0, arg1, arg2, activate);
      }

      public async Task WriteLineAsync(string message, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(message, activate);
      }

      public async Task WriteLineAsync(string message, object arg0, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(message, arg0, activate);
      }

      public async Task WriteLineAsync(string message, object arg0, object arg1, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(message, arg0, arg1, activate);
      }

      public async Task WriteLineAsync(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(message, arg0, arg1, arg2, activate);
      }

      /// <inheritdoc />
      public async Task WriteLineAsync(bool predicate, string message, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(predicate, message, activate);
      }

      /// <inheritdoc />
      public async Task WriteLineAsync(bool predicate, string message, object arg0, bool activate = true)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         outputPaneWriter.WriteLine(predicate, message, arg0, activate);
      }
   }
}
