// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining a logging service writing to the VS output pane.
   /// </summary>
   internal interface ILoggerService
   {
      /// <summary>
      /// Writes the supplied message to the VS debug output pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteOutputDebugPane(string message, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteOutputExtensionPane(string message, bool activate = true);

      /// <summary>
      /// Writes the supplied message a custom pane.
      /// </summary>
      /// <param name="message">The message to write.</param>
      /// <param name="activate"><c>true</c> to activate the pane; else <c>false</c></param>
      void WriteLineOutputExtensionPane(string message, bool activate = true);
   }

   /// <summary>
   /// Defines the service type for the logger service.
   /// </summary>
   internal interface SLoggerService
   {

   }
}
