// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Extension methods for the <see cref="IAsyncServiceProvider"/> interface simplifying access to VS services.
   /// </summary>
   internal static class AsyncServiceProviderExtensions
   {
      /// <summary>
      /// Gets the automation model top level object service (aka DTE2).
      /// </summary>
      /// <param name="serviceProvider">The service provider to query.</param>
      /// <returns>The <see cref="DTE2"/> service interface. Never null.</returns>
      public static Task<DTE2> GetAutomationModelTopLevelObjectServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         return serviceProvider.GetServiceAsync<SDTE, DTE2>();
      }

      /// <summary>
      /// Gets the CPS (Common Project System) service.
      /// </summary>
      /// <param name="serviceProvider">The service provider to query.</param>
      /// <returns>The <see cref="IProjectService"/> service interface.</returns>
      public static async Task<IProjectService> GetProjectServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         IComponentModel componentModel = await serviceProvider.GetServiceAsync<SComponentModel, IComponentModel>();
         if (componentModel == null)
         {
            // MEF Component model is not available, should actually never happen
            return null;
         }

         // Get the CPS service
         var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
         return projectServiceAccessor?.GetProjectService();
      }
   }
}
