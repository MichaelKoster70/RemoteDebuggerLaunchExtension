// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher.VsInterop
{
   internal class VsShellFacade : IVsShellFacade
   {
      private readonly IServiceProvider serviceProvider;
      private IOutputPaneWriterService outputWriter;
      private IStatusbarService statusbar;
      private IWaitDialogFactoryService waitDialogFactoryReal;
      private IWaitDialogFactoryService waitDialogFactoryStub;
      private IVsShell shell;
      private string registryRoot;
      private string vsLocalAppDataPath;

      private const uint NumberToFetch = 1U;
      private const int VirtualRegistryRoot = -9002;
      private const int LocalAppDataPath = -9055;

      public VsShellFacade(IServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider;
      }

      /// <inheritdoc />
      public IOutputPaneWriterService GetOutputPaneWriter()
      {
         return outputWriter = outputWriter ?? new OutputPaneWriterService();
      }

      /// <inheritdoc />
      public IStatusbarService GetStatusbar()
      {
         return statusbar = statusbar ?? new StatusbarService();
      }

      /// <inheritdoc />
      public IWaitDialogFactoryService GetWaitDialog(bool useWaitDialog)
      {
         if (useWaitDialog)
         {
            return waitDialogFactoryReal = waitDialogFactoryReal ?? new WaitDialogFactoryService();
         }
         else
         {
            return waitDialogFactoryStub = waitDialogFactoryStub ?? new WaitDialogFactoryStubService(GetStatusbar());
         }
      }

      /// <inheritdoc />
      public IEnumerable<IVsDocumentPreviewer> GetDefaultBrowsers(bool skipBuiltIn)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         List<IVsDocumentPreviewer> defaultBrowsers = new List<IVsDocumentPreviewer>();
         IVsEnumDocumentPreviewers enumerator = serviceProvider.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument3>().DocumentPreviewersEnum;
         
         IVsDocumentPreviewer[] documentPreviewerArray = new IVsDocumentPreviewer[NumberToFetch];
         while (enumerator.Next(NumberToFetch, documentPreviewerArray, out uint fetched) == 0 && fetched == NumberToFetch)
         {
            var documentPreviewer = documentPreviewerArray[0];

            // built-in browsers have no 'Path' value assigned
            if (documentPreviewer.IsDefault && (!skipBuiltIn || !string.IsNullOrEmpty(documentPreviewer.Path)))
            {
               defaultBrowsers.Add(documentPreviewer);
            }
         }

         return defaultBrowsers;
      }

      /// <inheritdoc />
      public string GetVSRegistryRoot()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (registryRoot == null && Shell.GetProperty(VirtualRegistryRoot, out object propertyValue) == 0)
         {
            registryRoot = propertyValue as string;
         }

         return registryRoot;
      }

      /// <inheritdoc />
      public string GetLocalAppDataPath()
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         if (vsLocalAppDataPath == null && Shell.GetProperty(LocalAppDataPath, out object propertyValue) == 0)
         {
            vsLocalAppDataPath = propertyValue as string;
         }

         return vsLocalAppDataPath;
      }

      private IVsShell Shell
      {
         get
         {
            if (shell == null)
            {
               shell = serviceProvider.GetService<SVsShell, IVsShell>();
            }

            return shell ?? throw new InvalidOperationException();
         }
      }
   }
}
