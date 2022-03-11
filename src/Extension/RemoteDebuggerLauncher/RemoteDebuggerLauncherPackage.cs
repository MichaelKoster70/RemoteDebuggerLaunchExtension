// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// This is the class that implements the package exposed by this assembly.
   /// </summary>
   /// <remarks>
   /// <para>
   /// The minimum requirement for a class to be considered a valid package for Visual Studio  is to implement the IVsPackage interface and register itself with the shell.
   /// This package uses the helper classes defined inside the Managed Package Framework (MPF) to do it: it derives from the Package class that provides the implementation of the
   /// IVsPackage interface and uses the registration attributes defined in the framework to register itself and its components with the shell. 
   /// These attributes tell the pkgdef creation utility what data to put into .pkgdef file.
   /// </para>
   /// <para>
   /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
   /// </para>
   /// </remarks>
   [ProvideBindingPath]
   [ProvideService(typeof(SOptionsPageAccessor), IsAsyncQueryable = true)]
   [ProvideService(typeof(SLoggerService), IsAsyncQueryable = true)]
   [InstalledProductRegistration("#110", "#112", Generated.AssemblyVersion.Version, IconResourceID = 400)]
   [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
   [ProvideOptionPage(typeof(RemoteDebuggerLauncherDeviceOptionsPage), PackageConstants.OptionsCategory, PackageConstants.OptionsPageDevice, 0, 0, true)]
   [ProvideOptionPage(typeof(RemoteDebuggerLauncherLocalOptionsPage), PackageConstants.OptionsCategory, PackageConstants.OptionsPageLocal, 0, 0, true)]
   [ProvideProfile(typeof(RemoteDebuggerLauncherDeviceOptionsPage), PackageConstants.OptionsCategory, "My Settings", 106, 107, isToolsOptionPage: true, DescriptionResourceID = 108)]
   [Guid(RemoteDebuggerLauncherPackage.PackageGuidString)]
   public sealed class RemoteDebuggerLauncherPackage : AsyncPackage
   {
      /// <summary>RemoteDebuggerLauncherPackage GUID string.</summary>
      public const string PackageGuidString = "624a755d-54e4-4069-84ec-e63cb53582f5";

      #region Package Members
      /// <summary>
      /// Initialization of the package; this method is called right after the package is sited, so this is the place
      /// where you can put all the initialization code that rely on services provided by VisualStudio.
      /// </summary>
      /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
      /// <param name="progress">A provider for progress updates.</param>
      /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
      protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
      {
         await base.InitializeAsync(cancellationToken, progress);

         AddService(typeof(SOptionsPageAccessor), CreateServiceAsync, true);
         AddService(typeof(SLoggerService), CreateServiceAsync, true);

         // When initialized asynchronously, the current thread may be a background thread at this point.
         // Do any initialization that requires the UI thread after switching to the UI thread.
         await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
      }
      #endregion

      #region private Methods
      private  Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken,Type serviceType)
      {
         if (typeof(SOptionsPageAccessor) == serviceType)
         {
            var serviceInstance = new OptionsPageAccessorService(this);
            return Task.FromResult<object>(serviceInstance);
         }
         else if(typeof(SLoggerService) == serviceType)
         {
            var serviceInstance = new LoggerService();
            return Task.FromResult<object>(serviceInstance);
         }

         return Task.FromResult<object>(null);
      }
      #endregion
   }
}
