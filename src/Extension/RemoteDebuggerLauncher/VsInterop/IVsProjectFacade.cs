// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the Visual Studio Project facade.
   /// </summary>
   internal interface IVsProjectFacade
   {
      /// <summary>
      /// Gets the VS project hierarchy.
      /// </summary>
      /// <value>A <see cref="IVsHierarchy"/> instance. Never null.</value>
      IVsHierarchy ProjectHierarchy { get; }

      /// <summary>
      /// Gets the project GUID as stored in the solution.
      /// </summary>
      /// <value>The project <see cref="Guid"/>.</value>
      Guid ProjectGuid { get; }

      /// <summary>
      /// Gets the project GUID as string.
      /// </summary>
      /// <value>The project GUID string.</value>
      string ProjectGuidString { get; }
   }
}
