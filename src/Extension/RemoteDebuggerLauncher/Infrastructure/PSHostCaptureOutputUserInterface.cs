// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;

namespace RemoteDebuggerLauncher.PowerShellHost
{
   /// <summary>
   /// Provides a PowerShell Host UI storing all script outputs.
   /// Implements the <see cref="PSHostUserInterface" /> interface.
   /// </summary>
   /// <seealso cref="PSHostUserInterface" />
   internal class PSHostOutputCaptureUserInterface : PSHostUserInterface
   {
      #region Private Fields
      private readonly PSHostConsoleRawUserInterface rawUi = new PSHostConsoleRawUserInterface();
      private readonly StringBuilder outputText = new StringBuilder();
      private readonly StringBuilder errorText = new StringBuilder();
      private readonly StringBuilder warningText = new StringBuilder();
      private readonly StringBuilder verboseText = new StringBuilder();
      private readonly StringBuilder debugText = new StringBuilder();

      private IList<string> outputLines;
      #endregion

      #region PSHostUserInterface Properties
      /// <summary>
      /// Gets an instance of the PSRawUserInterface class for this host application.
      /// </summary>
      /// <value>A reference to an instance of the hosting application's implementation of a class derived from
      /// <see cref="PSHostUserInterface" />, or null to indicate that low-level user interaction is not supported.</value>
      public override PSHostRawUserInterface RawUI => rawUi;
      #endregion

      #region OutputCapturePSHostUserInterface Properties
      /// <summary>
      /// Gets the output lines written by the executed commands.
      /// </summary>
      /// <value>The output lines.</value>
      public IList<string> OutputLines
      {
         get
         {
            if (outputLines == null)
            {
               outputLines = outputText.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            return outputLines;
         }
      }

      /// <summary>
      /// Gets the error text.
      /// </summary>
      public string ErrorText => errorText.ToString();
      #endregion

      #region PSHostUserInterface Methods
      /// <summary>
      /// Prompts the user for input.
      /// </summary>
      /// <param name="caption">The caption or title of the prompt.</param>
      /// <param name="message">The text of the prompt.</param>
      /// <param name="descriptions">A collection of FieldDescription objects that
      /// describe each field of the prompt.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }

      /// <summary>
      /// Provides a set of choices that enable the user to choose a single option from a set of options.
      /// </summary>
      /// <param name="caption">Text that proceeds (a title) the choices.</param>
      /// <param name="message">A message that describes the choice.</param>
      /// <param name="choices">A collection of ChoiceDescription objects that describes
      /// each choice.</param>
      /// <param name="defaultChoice">The index of the label in the Choices parameter collection. To indicate no default choice, set to -1.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }

      /// <summary>
      /// Prompts the user for credentials with a specified prompt window caption, prompt message, user name, and target name. 
      /// </summary>
      /// <param name="caption">The caption for the message window.</param>
      /// <param name="message">The text of the message.</param>
      /// <param name="userName">The user name whose credential is to be prompted for.</param>
      /// <param name="targetName">The name of the target for which the credential is collected.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }

      /// <summary>
      /// Prompts the user for credentials by using a specified prompt window caption, prompt message, user name and target name, credential types allowed to be
      /// returned, and UI behavior options.
      /// </summary>
      /// <param name="caption">The caption for the message window.</param>
      /// <param name="message">The text of the message.</param>
      /// <param name="userName">The user name whose credential is to be prompted for.</param>
      /// <param name="targetName">The name of the target for which the credential is collected.</param>
      /// <param name="allowedCredentialTypes">A PSCredentialTypes constant that identifies the type of credentials that can be returned.</param>
      /// <param name="options">A PSCredentialUIOptions constant that identifies the UI behavior when it gathers the credentials.</param>
      /// <returns>Throws a NotImplementedException exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }

      /// <summary>
      /// Reads characters that are entered by the user until a newline is encountered.
      /// </summary>
      /// <returns>The characters that are entered by the user.</returns>
      public override string ReadLine()
      {
         return string.Empty;
      }

      /// <summary>
      /// Reads characters entered by the user until a newline is encountered and returns the characters as a secure string.
      /// </summary>
      /// <returns>Throws a NotImplemented exception.</returns>
      /// <exception cref="System.NotImplementedException">The method or operation is not implemented.</exception>
      public override SecureString ReadLineAsSecureString()
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }

      /// <summary>
      /// Writes characters to the output display of the host.
      /// </summary>
      /// <param name="value">The characters to be written.</param>
      public override void Write(string value)
      {
         _ = outputText.Append(value);
      }

      /// <summary>
      /// Writes characters to the output display of the host and specifies the foreground and background colors of the characters. 
      /// This implementation ignores the colors.
      /// </summary>
      /// <param name="foregroundColor">The color of the characters.</param>
      /// <param name="backgroundColor">The background color to use.</param>
      /// <param name="value">The characters to be written.</param>
      public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
      {
         _ = outputText.Append(value);
      }

      /// <summary>
      /// Writes a debug message to the output display of the host.
      /// </summary>
      /// <param name="message">The debug message that is displayed.</param>
      public override void WriteDebugLine(string message)
      {
         _ = debugText.AppendLine(message);
      }

      /// <summary>
      /// Writes an error message to the output display of the host.
      /// </summary>
      /// <param name="value">The error message that is displayed.</param>
      public override void WriteErrorLine(string value)
      {
         _ = errorText.AppendLine(value);
      }

      /// <summary>
      /// Writes a line of characters to the output display of the host and appends a newline character(carriage return).
      /// </summary>
      /// <param name="value">The line to be written.</param>
      public override void WriteLine(string value)
      {
         _ = outputText.AppendLine(value);
      }

      /// <summary>
      /// Writes a line of characters to the output display of the host with foreground and background colors and appends a newline (carriage return).
      /// </summary>
      /// <param name="foregroundColor">The foreground color of the display. </param>
      /// <param name="backgroundColor">The background color of the display. </param>
      /// <param name="value">The line to be written.</param>
      public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
      {
         _ = outputText.AppendLine(value);
      }

      /// <summary>
      /// Writes a progress report to the output display of the host.
      /// </summary>
      /// <param name="sourceId">Unique identifier of the source of the record. </param>
      /// <param name="record">A ProgressReport object.</param>
      public override void WriteProgress(long sourceId, ProgressRecord record)
      {
         //no implementation
      }

      /// <summary>
      /// Writes a verbose message to the output display of the host.
      /// </summary>
      /// <param name="message">The verbose message that is displayed.</param>
      public override void WriteVerboseLine(string message)
      {
         _ = verboseText.AppendLine(message);
      }

      /// <summary>
      /// Writes a warning message to the output display of the host.
      /// </summary>
      /// <param name="message">The warning message that is displayed.</param>
      public override void WriteWarningLine(string message)
      {
         _ = warningText.AppendLine(message);
      }
      #endregion

      #region OutputCapturePSHostUserInterface Methods
      public void Reset()
      {
         _ = outputText.Clear();
         _ = errorText.Clear();
         _ = warningText.Clear();
         _ = verboseText.Clear();
         _ = debugText.Clear();
         outputLines = null;
      }
      #endregion
   }
}
