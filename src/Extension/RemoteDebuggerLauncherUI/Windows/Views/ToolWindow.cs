// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// This class implements the tool window exposed by this package and hosts a user control.
   /// </summary>
   /// <remarks>
   /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
   /// usually implemented by the package implementer.
   /// <para>
   /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
   /// implementation of the IVsUIElementPane interface.
   /// </para>
   /// </remarks>
   [Guid("531eb418-552a-4d50-888c-727a66520d75")]
   public class ToolWindow : ToolWindowPane
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="ToolWindow"/> class.
      /// </summary>
      public ToolWindow() : base(null)
      {
         Caption = Resources.ToolWindowCaption;

         // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
         // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
         // the object returned by the Content property.
         Content = new ToolWindowControl();
      }
   }
}
