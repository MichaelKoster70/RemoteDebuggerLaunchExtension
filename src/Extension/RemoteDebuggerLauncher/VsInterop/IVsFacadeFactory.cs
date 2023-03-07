// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Factory for Visual Studio Interop facades.
   /// </summary>
   /// <remarks>exposed as MEF component.</remarks>
   internal interface IVsFacadeFactory
   {
      /// <summary>Gets the VS project facade for the supplied COM object.</summary>
      /// <param name="hierarchy">The VS hierarchy representing the project.</param>
      /// <returns>An <see cref="IVsProjectFacade"/> instance. Never null.</returns>
      IVsProjectFacade GetVsProject(IVsHierarchy hierarchy);

      /// <summary>Gets the VS project facade for the supplied COM object.</summary>
      /// <param name="configuredProject">The CPS project.</param>
      /// <returns>An <see cref="IVsProjectFacade"/> instance. Never null.</returns>
      IVsProjectFacade GetVsProject(ConfiguredProject configuredProject);

      /// <summary>
      /// Gets the facade for the loaded Visual Studio solution.
      /// </summary>
      /// <returns>A <see cref="IVsSolutionFacade"/> instance. Never null.</returns>
      IVsSolutionFacade GetVsSolution();

      /// <summary>
      /// Gets the facade for the Visual Studio shell services.
      /// </summary>
      /// <returns>A <see cref="IVsShellFacade"/> instance. Never null.</returns>
      IVsShellFacade GetVsShell();

      /// <summary>
      /// Gets the facade for the Visual Studio debugger services.
      /// </summary>
      /// <returns>A <see cref="IVsDebuggerFacade"/> instance. Never null.</returns>
      IVsDebuggerFacade GetVsDebugger();
   }
}
