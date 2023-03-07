// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Data container holding Web Browser information.
   /// </summary>
   internal class WebBrowserInfo : IDisplayNameProvider
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="WebBrowserInfo"/> class with the supplied arguments.
      /// </summary>
      /// <param name="kind">The browser kind.</param>
      /// <param name="displayName">The display name.</param>
      /// <param name="path">The path to the browser binary.</param>
      /// <param name="arguments">The arguments to be passed when launching the browser.</param>
      /// <param name="newWindowArgument">The arguments needed to launch the browser in a new window.</param>
      public WebBrowserInfo(BrowserKind kind, string displayName, string path, string arguments, string newWindowArgument = "")
      {
         Kind = kind;
         DisplayName = displayName;
         Path = path;
         Arguments = arguments ?? string.Empty;
         NewWindowArgument = newWindowArgument;
      }

      /// <summary>
      /// Gets the kind of browser.
      /// </summary>
      /// <value>One of the <see cref="BrowserKind"/> values.</value>
      public BrowserKind Kind { get; }

      /// <summary>
      /// Gets the display name.
      /// </summary>
      public string DisplayName { get; }

      /// <summary>
      /// Gets the path to the browser binary.
      /// </summary>
      public string Path { get; }

      /// <summary>
      /// Gets the arguments to be passed when launching the browser.
      /// </summary>
      public string Arguments { get; }

      /// <summary>
      /// Gets the arguments needed to launch the browser in a new window.
      /// </summary>
      public string NewWindowArgument { get; }

      /// <summary>
      /// Gets the combined command line arguments needed to launch the browser in a new window.
      /// </summary>
      public string NewWindowCommandLineArguments => Arguments != null ? NewWindowArgument + Arguments : NewWindowArgument;
   }
}
