// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Interface for the Web Browser selection service.
   /// </summary>
   internal interface IWebBrowserSelectionService
   {
      /// <summary>
      /// Gets the default browsers selected in Visual Studio.
      /// </summary>
      /// <returns>A list of <see cref="WebBrowserInfo"/>; an empty list if no browsers are selected.</returns>
      IList<WebBrowserInfo> GetDefaultBrowsers();

      /// <summary>
      /// Gets the browser for a debugging session as selected in Visual Studio.
      /// </summary>
      /// <returns></returns>
      /// <exception cref="RemoteDebuggerLauncherException">Thrown if no browser is configured</exception>
      WebBrowserInfo GetDefaultBrowserForDebug();
   }
}
