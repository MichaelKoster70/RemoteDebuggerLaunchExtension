// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining the configured projects package service factory.
   /// Exposes the .NET publish, deploy, SSH remote operations services.
   /// </summary>
   /// <seealso cref="IPackageServiceFactory"/>
   internal interface IConfiguredPackageServiceFactory : IPackageServiceFactory
   {
      /// <summary>
      /// Configures the service with the given launch profile and the default output pane writer.
      /// </summary>
      /// <param name="launchProfile">The </param>
      /// <returns>A task representing the operation.</returns>
      Task ConfigureAsync(ILaunchProfile launchProfile);

      /// <summary>
      /// Configures the service using the active launch profile with the given output pane writer.
      /// </summary>
      /// <param name="textWriter">The text writer to use as output</param>
      /// <returns>A task representing the operation.</returns>
      Task ConfigureAsync(TextWriter textWriter);
   }
}
