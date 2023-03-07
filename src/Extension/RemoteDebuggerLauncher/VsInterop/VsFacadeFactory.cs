// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RemoteDebuggerLauncher.VsInterop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Visual Studio Interop facade factory implementation.
   /// </summary>
   [Export(typeof(IVsFacadeFactory))]
   internal class VsFacadeFactory : IVsFacadeFactory
   {
      private readonly IServiceProvider serviceProvider;
      private IVsSolutionFacade vsSolution;
      private IVsShellFacade vsShell;
      private IVsDebuggerFacade vsDebugger;

      /// <summary>
      /// Initializes a new instance of the <see cref="VsFacadeFactory"/> class.
      /// </summary>
      /// <param name="serviceProvider">The MEF injected service provider.</param>
      [ImportingConstructor]
      public VsFacadeFactory(SVsServiceProvider serviceProvider)
      {
         this.serviceProvider = serviceProvider;
      }

      /// <inheritdoc />
      public IVsProjectFacade GetVsProject(IVsHierarchy hierarchy) => hierarchy != null ? new VsProjectFacade(serviceProvider, hierarchy) : throw new ArgumentNullException(nameof(hierarchy));

      /// <inheritdoc />
      public IVsProjectFacade GetVsProject(ConfiguredProject configuredProject)
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         return GetVsProject(configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy);
      }

      /// <inheritdoc />
      public IVsSolutionFacade GetVsSolution() => vsSolution = vsSolution ?? new VsSolutionFacade(serviceProvider);

      /// <inheritdoc />
      public IVsShellFacade GetVsShell() => vsShell = vsShell ?? new VsShellFacade(serviceProvider);

      /// <inheritdoc />
      public IVsDebuggerFacade GetVsDebugger() => vsDebugger = vsDebugger ?? new VsDebuggerFacade(serviceProvider);
   }
}
