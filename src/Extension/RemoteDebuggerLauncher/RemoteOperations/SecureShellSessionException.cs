// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace RemoteDebuggerLauncher.RemoteOperations
{
   /// <summary>
   /// Exception thrown when a SSH remote command fails.
   /// </summary>
   /// <seealso cref="System.InvalidOperationException" />
   internal class SecureShellSessionException : InvalidOperationException
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionException"/> class.
      /// </summary>
      public SecureShellSessionException()
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionException"/> class with error message and exit code.
      /// </summary>
      /// <param name="message">The error message of the command</param>
      /// <param name="exitCode">The exit code.</param>
      public SecureShellSessionException(string message, int exitCode) : base(message)
      {
         ExitCode = exitCode;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionException"/> class.
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public SecureShellSessionException(string message) : base(message)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionException"/> class.
      /// </summary>
      /// <param name="message">The error message that explains the reason for the exception.</param>
      /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference (<see langword="Nothing" /> in Visual Basic), the current exception is raised in a <see langword="catch" /> block that handles the inner exception.</param>
      public SecureShellSessionException(string message, Exception innerException) : base(message, innerException)
      {
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionException"/> class.
      /// </summary>
      /// <param name="info">The object that holds the serialized object data.</param>
      /// <param name="context">The contextual information about the source or destination.</param>
      protected SecureShellSessionException(SerializationInfo info, StreamingContext context) : base(info, context)
      {
      }

      /// <summary>
      /// Gets the exit code of the command.
      /// </summary>
      public int ExitCode { get; private set; } = -1;
   }
}
