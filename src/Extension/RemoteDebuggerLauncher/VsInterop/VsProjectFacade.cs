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
   /// Implements the Visual Studio Project facade.
   /// Implements the <see cref="IVsProjectFacade" />
   /// </summary>
   /// <seealso cref="IVsProjectFacade" />
   internal class VsProjectFacade : IVsProjectFacade
   {
      private readonly IServiceProvider serviceProvider;
      private Guid projectGuid;

      /// <summary>
      /// Initializes a new instance of the <see cref="VsProjectFacade"/> class.
      /// </summary>
      /// <param name="serviceProvider">The service provider.</param>
      /// <param name="projectHierarchy">The project hierarchy.</param>
      public VsProjectFacade(IServiceProvider serviceProvider, IVsHierarchy projectHierarchy)
      {
         this.serviceProvider = serviceProvider;
         ProjectHierarchy = projectHierarchy;
      }

      /// <inheritdoc />
      public IVsHierarchy ProjectHierarchy { get; }

      /// <inheritdoc />
      public Guid ProjectGuid
      {
         get
         {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectGuid == Guid.Empty)
            {
               _ = ErrorHandler.ThrowOnFailure(serviceProvider.GetService<SVsSolution, IVsSolution>().GetGuidOfProject(ProjectHierarchy, out projectGuid));
            }
            return projectGuid;
         }
      }

      /// <inheritdoc />
      public string ProjectGuidString
      { 
         get
         {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ProjectGuid.ToString();
         }
      }
   }
}
