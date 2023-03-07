// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Web Browser selection service implementation.
   /// Implements the <see cref=IWebBrowserSelectionService" />
   /// </summary>
   /// <seealso cref="IWebBrowserSelectionService"/>
   [Export(typeof(IWebBrowserSelectionService))]
   [AppliesTo("DotNetCoreWeb")]
   internal class WebBrowserSelectionService : IWebBrowserSelectionService
   {
      private readonly IVsFacadeFactory facadeFactory;

      [ImportingConstructor]
      public WebBrowserSelectionService(IVsFacadeFactory facadeFactory)
      {
         this.facadeFactory = facadeFactory;
      }

      /// <inheritdoc />
      public IList<WebBrowserInfo> GetDefaultBrowsers()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var defaultBrowsers = GetDefaultExternalBrowsers();

         if (defaultBrowsers.Count == 0)
         {
            throw new RemoteDebuggerLauncherException(ExceptionMessages.NoExternalBrowserConfigured);
         }

         return defaultBrowsers;
      }

      /// <inheritdoc />
      public WebBrowserInfo GetDefaultBrowserForDebug()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         var defaultBrowsers = GetDefaultExternalBrowsers();

         if (defaultBrowsers.Count == 1)
         {
            return defaultBrowsers[0];
         }
         else if (defaultBrowsers.Count > 1)
         {
            // more than 1 browser selected as default => Bring up dialog to select the browser to use
            var viewModel = new SelectBrowserViewModel(ThreadHelper.JoinableTaskFactory);

            foreach(var browser in defaultBrowsers)
            {
               viewModel.Browsers.Add(new BrowserViewModel(browser));
            }

            var dialog = new SelectBrowserDialogWindow()
            {
               DataContext = viewModel
            };

            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
               return viewModel.SelectedBrowser.GetContext<WebBrowserInfo>();
            }
            else
            {
               // no browser got selected
               throw new RemoteDebuggerLauncherException(ExceptionMessages.NoExternalBrowserSelected);
            }
         }
         else
         {
            // no browser configured in VS
            throw new RemoteDebuggerLauncherException(ExceptionMessages.NoExternalBrowserConfigured);
         }
      }

      private List<WebBrowserInfo> GetDefaultExternalBrowsers()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         List<WebBrowserInfo> defaultBrowsers = new List<WebBrowserInfo>();
         foreach (IVsDocumentPreviewer defaultBrowser in facadeFactory.GetVsShell().GetDefaultBrowsers(true))
         {
            var unquotedPath = defaultBrowser.Path.Unquote();
            var browserTarget = BrowserTargetDetectorFactory.GetDebugTargetByLauncher(unquotedPath);
            defaultBrowsers.Add(new WebBrowserInfo(browserTarget.Kind, defaultBrowser.DisplayName, unquotedPath, defaultBrowser.Arguments, browserTarget.NewWindowCommandLineArgument));
         }

         return defaultBrowsers;
      }
   }
}
