// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;

namespace RemoteDebuggerLauncher.PowerShellHost
{
   /// <summary>
   /// Implements a PowerShell Host capturing the script outputs.
   /// </summary>
   /// <seealso cref="PSHost" />
   internal class CaptureOutputPSHost : PSHost
   {
      #region Private Fields
      /// <summary>The identifier of this Powershell host implementation.</summary>
      private Guid instanceId = Guid.NewGuid();

      /// <summary>The culture information of the thread that created this object.</summary>
      private readonly CultureInfo originalCultureInfo = Thread.CurrentThread.CurrentCulture;

      /// <summary>The UI culture information of the thread that created this object.</summary>
      private readonly CultureInfo originalUICultureInfo = Thread.CurrentThread.CurrentUICulture;

      /// <summary>A reference to the implementation of the PSHostUserInterface.</summary>
      private readonly OutputCapturePSHostUserInterface hostUserInterface = new OutputCapturePSHostUserInterface();
      #endregion

      #region PSHost Properties
      /// <summary>
      /// Gets the culture information to use.
      /// </summary>
      /// <value>A CultureInfo object representing the host's current culture.</value>
      /// <remarks>The runspace will set the thread current culture to this value each time it starts a pipeline. 
      /// Thus, cmdlets are encouraged to use Thread.CurrentThread.CurrentCulture.</remarks>
      public override CultureInfo CurrentCulture => originalCultureInfo;

      /// <summary>
      /// Gets the UI culture information to use.
      /// </summary>
      /// <value>A CultureInfo object representing the host's current UI culture.</value>
      /// <remarks>This implementation returns a snapshot of the UI culture information of the thread that created this object.</remarks>
      public override CultureInfo CurrentUICulture => originalUICultureInfo;

      /// <summary>
      /// Gets an identifier for this host.
      /// </summary>
      /// <value>The instance identifier.</value>
      public override Guid InstanceId => instanceId;

      /// <summary>
      /// Gets a string that contains the name of this host implementation.
      /// </summary>
      /// <value>The name identifier of the hosting application.</value>
      public override string Name => nameof(CaptureOutputPSHost);

      /// <summary>
      /// Gets an instance of the implementation of the PSHostUserInterface class for this application. 
      /// This instance is allocated once at startup time and returned every time thereafter.
      /// </summary>
      /// <value>A reference to an instance of the hosting application's implementation of a class derived from
      /// <see cref="PSHostUserInterface" />, or null to indicate that user interaction is not supported.</value>
      public override PSHostUserInterface UI => hostUserInterface;

      /// <summary>
      /// Gets the version object for this application.
      /// </summary>
      /// <value>The version number of the hosting application.</value>
      public override Version Version => new Version(1, 0, 0, 0);
      #endregion

      #region CustomPSHost Properties
      /// <summary>
      /// Gets the output lines written by the executed commandss
      /// </summary>
      /// <value>The output lines.</value>
      public IList<string> OutputLines => hostUserInterface.OutputLines;

      /// <summary>
      /// Gets a value indicating whether this instance has error.
      /// </summary>
      /// <value><c>true</c> if this instance has error; otherwise, <c>false</c>.</value>
      public bool HasError => !string.IsNullOrEmpty(hostUserInterface.ErrorText);

      /// <summary>
      /// Gets the error text.
      /// </summary>
      /// <value>The error text.</value>
      public string ErrorText => hostUserInterface.ErrorText;
      #endregion

      #region PSHost Methods
      /// <summary>
      /// This API Instructs the host to interrupt the currently running pipeline and start a new nested input loop. 
      /// </summary>
      /// <remarks>Not implemented, throws an NotImplementedException exception.</remarks>
      public override void EnterNestedPrompt()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// This API instructs the host to exit the currently running input loop.
      /// </summary>
      /// <remarks>Not implemented, throws an NotImplementedException exception.</remarks>
      public override void ExitNestedPrompt()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// This API is called before an external application process is started. 
      /// </summary>
      public override void NotifyBeginApplication()
      {
        //EMPTY_BODY
      }

      /// <summary>
      /// This API is called after an external application process finishes.
      /// </summary>
      public override void NotifyEndApplication()
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Indicate to the host application that exit has been requested.
      /// Pass the exit code that the host application should use when exiting the process.
      /// </summary>
      /// <param name="exitCode">The exit code that the host application should use.</param>
      public override void SetShouldExit(int exitCode)
      {
         throw new NotImplementedException();
      }
      #endregion
   }
}
