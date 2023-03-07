// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Logging Service implementation writing an existing output pane.
   /// Implements the <see cref="IOutputPaneWriterService" />
   /// </summary>
   /// <seealso cref="IOutputPaneWriterService" />
   internal class OutputPaneTextWriterService : IOutputPaneWriterService
   {
      private readonly TextWriter outputPaneWriter;

      /// <summary>
      /// Initializes a new instance of the <see cref="OutputPaneTextWriterService"/> class.
      /// </summary>
      public OutputPaneTextWriterService(TextWriter outputPaneWriter)
      {
         this.outputPaneWriter = outputPaneWriter ?? throw new ArgumentNullException(nameof(outputPaneWriter));
      }

      /// <inheritdoc />
      public void Write(string message, bool activate = true)
      {
         outputPaneWriter.Write(message);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, bool activate = true)
      {
         Write(string.Format(message, arg0), activate);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, object arg1, bool activate = true)
      {
         Write(string.Format(message, arg0, arg1), activate);
      }

      /// <inheritdoc />
      public void Write(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         Write(string.Format(message, arg0, arg1, arg2), activate);
      }

      /// <inheritdoc />
      public void Write(bool predicate, string message, object arg0, object arg1, bool activate = true)
      {
         if (predicate)
         {
            Write(string.Format(message, arg0, arg1), activate);
         }
      }

      /// <inheritdoc />
      public void Write(bool predicate, string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         if (predicate)
         {
            Write(string.Format(message, arg0, arg1, arg2), activate);
         }
      }

      /// <inheritdoc />
      public void WriteLine(string message, bool activate = true)
      {
         outputPaneWriter.WriteLine(message);
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, bool activate = true)
      {
         WriteLine(string.Format(message, arg0), activate);
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, object arg1, bool activate = true)
      {
         WriteLine(string.Format(message, arg0, arg1), activate);
      }

      /// <inheritdoc />
      public void WriteLine(string message, object arg0, object arg1, object arg2, bool activate = true)
      {
         WriteLine(string.Format(message, arg0, arg1, arg2), activate);
      }

      /// <inheritdoc />
      public void WriteLine(bool predicate, string message, bool activate = true )
      {
         if (predicate)
         {
            WriteLine(message, activate);
         }
      }

      /// <inheritdoc />
      public void WriteLine(bool predicate, string message, object arg0, bool activate = true)
      {
         WriteLine(predicate, string.Format(message, arg0), activate);
      }
   }
}
