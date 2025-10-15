// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using RemoteDebuggerLauncher.RemoteOperations;
using RemoteDebuggerLauncher.WebTools;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Extension methods for the <see cref="IAsyncServiceProvider"/> interface simplifying access to Visual Studio services.
   /// </summary>
   internal static class AsyncServiceProviderExtensions
   {
      /// <summary>
      /// Gets the Options Page Accessor service interface from a service provider.
      /// </summary>
      /// <param name="asyncServiceProvider">The service provider to query.</param>
      /// <returns>The <see cref="IOptionsPageAccessor"/> service interface. Never null.</returns>
      /// <exception cref="ServiceUnavailableException">The service could not be acquired.<exception>
      public static Task<IOptionsPageAccessor> GetOptionsPageServiceAsync(this IAsyncServiceProvider asyncServiceProvider)
      {
         return asyncServiceProvider.GetServiceAsync<SOptionsPageAccessor, IOptionsPageAccessor>();
      }

      /// <summary>
      /// Gets the CPS (Common Project System) service.
      /// </summary>
      /// <param name="serviceProvider">The service provider to query.</param>
      /// <returns>The <see cref="IProjectService"/> service interface. Never null.</returns>
      public static async Task<IProjectService> GetProjectServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         IComponentModel componentModel = await serviceProvider.GetComponentModelAsync();
         if (componentModel == null)
         {
            // MEF Component model is not available, should actually never happen
            return null;
         }

         // Get the CPS service
         var projectServiceAccessor = componentModel.GetService<IProjectServiceAccessor>();
         return projectServiceAccessor?.GetProjectService();
      }

      /// <summary>
      /// Gets the Visual Studio facade factory.
      /// </summary>
      /// <param name="serviceProvider">The async service provider to use.</param>
      /// <returns>An <see cref="IVsFacadeFactory"/> instance. Never null.</returns>
      public static async Task<IVsFacadeFactory> GeVsFacadeFactoryAsync(this IAsyncServiceProvider serviceProvider)
      {
         IComponentModel componentModel = await serviceProvider.GetComponentModelAsync();

         return componentModel.GetService<IVsFacadeFactory>();
      }

      public static async Task<ISecureShellKeySetupService> GetSecureShellKeySetupServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         IComponentModel componentModel = await serviceProvider.GetComponentModelAsync();

         return componentModel.GetService<ISecureShellKeySetupService>();
      }

      public static async Task<ICertificateService> GetCertificateServiceAsync(this IAsyncServiceProvider serviceProvider)
      {
         IComponentModel componentModel = await serviceProvider.GetComponentModelAsync();

         return componentModel.GetService<ICertificateService>();
      }

      private static Task<IComponentModel> GetComponentModelAsync(this IAsyncServiceProvider serviceProvider)
      {
         return serviceProvider.GetServiceAsync<SComponentModel, IComponentModel>();
      }
   }
}
