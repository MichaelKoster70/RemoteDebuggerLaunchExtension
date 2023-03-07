// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining a service writing to the VS output pane.
   /// </summary>
   internal interface IOutputPaneWriterService
   {
      /// <summary>
      /// Writes the supplied message to the output pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(string message, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(string message, object arg0, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg1">The second object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(string message, object arg0, object arg1, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg1">The second object to format.</param>
      /// <param name="arg2">The third object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(string message, object arg0, object arg1, object arg2, bool activate = true);

      /// <summary>
      /// Writes the supplied message to the output pane.
      /// </summary>
      /// <param name="predicate">predicate whether to write the message or not.</param>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg1">The second object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(bool predicate, string message, object arg0, object arg1, bool activate = true);

      /// <summary>
      /// Writes the supplied message to the output pane.
      /// </summary>
      /// <param name="predicate">predicate whether to write the message or not.</param>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg1">The second object to format.</param>
      /// <param name="arg2">The second object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void Write(bool predicate, string message, object arg0, object arg1, object arg2, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(string message, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(string message, object arg0, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg2">The third object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(string message, object arg0, object arg1, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="arg1">The second object to format.</param>
      /// <param name="arg2">The third object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(string message, object arg0, object arg1, object arg2, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="predicate">predicate whether to write the message or not.</param>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(bool predicate, string message, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="predicate">predicate whether to write the message or not.</param>
      /// <param name="message">The message to write.</param>
      /// <param name="arg0">The first object to format.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLine(bool predicate, string message, object arg0, bool activate = true);
   }
}
