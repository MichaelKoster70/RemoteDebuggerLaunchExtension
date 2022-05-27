// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining a service writing to the VS status bar.
   /// </summary>
   internal interface IStatusbarService
   {
      /// <summary>
      /// Sets the status bar text in the text area.
      /// </summary>
      /// <param name="text">The text to display in the status text area.</param>
      void SetText(string text);
   }

   /// <summary>
   /// Defines the service type for the status bar service.
   /// </summary>
   [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "By design, using VS naming standards")]
   internal interface SStatusbarService
   {
   }
}
