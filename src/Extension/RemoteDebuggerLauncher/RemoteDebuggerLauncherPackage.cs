﻿// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

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
   [InstalledProductRegistration("#110", "#112", Generated.AssemblyVersion.Version, IconResourceID = 400)]
   [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
   [ProvideOptionPage(typeof(DeviceOptionsPage), PackageConstants.Options.Category, PackageConstants.Options.PageNameDevice, 100, 101, true)]
   [ProvideOptionPage(typeof(LocalOptionsPage), PackageConstants.Options.Category, PackageConstants.Options.PageNameLocal, 100, 102, true)]
   [ProvideProfile(typeof(DeviceOptionsPage), PackageConstants.Options.Category, PackageConstants.Options.PageNameDevice, 200, 201, true, DescriptionResourceID = 202)]
   [ProvideProfile(typeof(LocalOptionsPage), PackageConstants.Options.Category, PackageConstants.Options.PageNameLocal, 200, 203, true, DescriptionResourceID = 204)]
   [Guid(RemoteDebuggerLauncherPackage.PackageGuidString)]
   [ProvideMenuResource("Menus.ctmenu", 1)]
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

         // Add services implemented in this package
         AddService(typeof(SOptionsPageAccessor), CreateServiceAsync, true);

         // When initialized asynchronously, the current thread may be a background thread at this point.
         // Do any initialization that requires the UI thread after switching to the UI thread.
         await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

         await CleanOutputCommand.InitializeAsync(this);
         await DeployOutputCommand.InitializeAsync(this);
         await InstallDebuggerCommand.InitializeAsync(this);
         await InstallDotnetCommand.InitializeAsync(this);
         await SetupSshCommand.InitializeAsync(this);
         await SetupHttpsCommand.InitializeAsync(this);
      }
      #endregion

      #region Private Methods
      /// Factory methods responsible to create service instances registered in this package.
      /// </summary>
      /// <param name="container">The container holding the services.</param>
      /// <param name="cancellationToken">A cancellation token to monitor for creation cancellation.</param>
      /// <param name="serviceType">The type of service to create.</param>
      /// <returns>A task representing the service instance. <c>null</c> if the service cannot be found.</returns>
      private Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken,Type serviceType)
      {
         if (typeof(SOptionsPageAccessor) == serviceType)
         {
            return Task.FromResult<object>(new OptionsPageAccessorService(this));
         }

         return Task.FromResult<object>(null);
      }
      #endregion
   }
}
