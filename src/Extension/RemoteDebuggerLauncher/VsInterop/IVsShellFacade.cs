// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   internal interface IVsShellFacade
   {
      /// <summary>
      /// Get the VS output pane writer service.
      /// </summary>
      /// <returns>The <see cref="IStatusbarService"/> instance.</returns>
      IOutputPaneWriterService GetOutputPaneWriter();

      /// <summary>
      /// Get the VS statusbar wrapper service.
      /// </summary>
      /// <returns>The <see cref="IStatusbarService"/> instance.</returns>
      IStatusbarService GetStatusbar();

      IWaitDialogFactoryService GetWaitDialog(bool useWaitDialog);

      /// <summary>
      /// Gets the Web browsers marked as 'Default'.
      /// </summary>
      /// <param name="skipBuiltIn"><c>true</c> to skip the VS built-in browser (aka IE); else <c>false</c></param>
      /// <returns>The collection of <see cref="IVsDocumentPreviewer"/> representing the browsers.</returns>
      IEnumerable<IVsDocumentPreviewer> GetDefaultBrowsers(bool skipBuiltIn);

      /// <summary>
      /// Get the Visual Studio local app data path.
      /// </summary>
      /// <returns>A <see langword=""="string"/> holding the path.</returns>
      string GetLocalAppDataPath();

      string GetVSRegistryRoot();
   }
}
