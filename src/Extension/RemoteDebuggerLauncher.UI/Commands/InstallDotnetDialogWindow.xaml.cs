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
using Microsoft.VisualStudio.PlatformUI;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Modal Dialog providing the UI options for the Install .NET Command.
   /// Implements the <see cref="Microsoft.VisualStudio.PlatformUI.DialogWindow" />
   /// </summary>
   /// <seealso cref="Microsoft.VisualStudio.PlatformUI.DialogWindow" />
   public partial class InstallDotnetDialogWindow : DialogWindow
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="InstallDotnetDialogWindow"/> class.
      /// </summary>
      public InstallDotnetDialogWindow()
      {
         InitializeComponent();
      }
   }
}

