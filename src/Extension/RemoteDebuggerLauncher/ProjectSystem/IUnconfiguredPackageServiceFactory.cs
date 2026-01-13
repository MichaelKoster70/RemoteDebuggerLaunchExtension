// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the unconfigured projects package service factory.
   /// Exposes the .NET publish, deploy, SSH remote operations services.
   /// </summary>
   /// <seealso cref="IPackageServiceFactory"/>
   internal interface IUnconfiguredPackageServiceFactory : IPackageServiceFactory
   {
      /// <summary>
      /// Gets the protect name name.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the name.</returns>
      string GetProjectName();

      /// <summary>
      /// Configures the service with the active launch profile and the default output pane writer.
      /// </summary>
      /// <returns><c>true</c> is active profile is supported, else <c>false</c></returns>
      Task<bool> ConfigureAsync();

      /// <summary>
      /// Gets the launch profile editor service.
      /// </summary>
      /// <returns>A <see cref="ILaunchProfileEditor"/> instance.</returns>
      Task<ILaunchProfileEditor> GetLaunchProfileEditorAsync();
   }
}
