// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Exception thrown when an unrecoverable error occurred.
   /// </summary>
   [Serializable]
   public class RemoteDebuggerLauncherException : Exception
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="RemoteDebuggerLauncherException"/> class.
      /// </summary>
      public RemoteDebuggerLauncherException()
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="RemoteDebuggerLauncherException"/> class with a specified error message.
      /// </summary>
      /// <param name="message">The error message that explains the reason for the exception.</param>
      public RemoteDebuggerLauncherException(string message) : base(message)
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="RemoteDebuggerLauncherException"/> class with a specified error
      ///  message and a reference to the inner exception that is the cause of this exception.
      /// </summary>
      /// <param name="message">The error message that explains the reason for the exception.</param>
      /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
      public RemoteDebuggerLauncherException(string message, Exception innerException) : base(message, innerException)
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="RemoteDebuggerLauncherException"/> class with serialized data.
      /// </summary>
      /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
      /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
      protected RemoteDebuggerLauncherException(SerializationInfo info, StreamingContext context) : base(info, context)
      {
         //EMPTY_BODY
      }
   }
}
