// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Responsible for listening to Visual Studio solution events and expose them as .NET events
   /// Implements the <see cref="IVsSolutionEvents" />
   /// Implements the <see cref="RemoteDebuggerLauncher.IVsSolutionEventsFacade" />
   /// </summary>
   /// <seealso cref="IVsSolutionEvents" />
   /// <seealso cref="RemoteDebuggerLauncher.IVsSolutionEventsFacade" />
   internal class VsSolutionEventsListener : IVsSolutionEvents, IVsSolutionEventsFacade, IDisposable
   {
      private readonly IVsSolution vsSolution;
      private uint cookie;

      public VsSolutionEventsListener(IVsSolution vsSolution)
      {
         this.vsSolution = vsSolution;

         // Advise for solution events
         ThreadHelper.ThrowIfNotOnUIThread();
         _ = vsSolution.AdviseSolutionEvents(this, out cookie);
      }

      /// <inheritdoc/>
      public event EventHandler AfterCloseSolution;

      public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.S_OK;
      public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.S_OK;
      public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.S_OK;
      public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.S_OK;
      public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.S_OK;
      public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.S_OK;
      public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => VSConstants.S_OK;
      public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;
      public int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.S_OK;
      public int OnAfterCloseSolution(object pUnkReserved)
      {
         AfterCloseSolution?.Invoke(this, EventArgs.Empty);
         return VSConstants.S_OK;
      }

      /// <inheritdoc />
      public void Dispose()
      {
         ThreadHelper.ThrowIfNotOnUIThread();
         if (cookie != 0)
         {
            _ = vsSolution.UnadviseSolutionEvents(cookie);
            cookie = 0;
         }
      }
   }
}