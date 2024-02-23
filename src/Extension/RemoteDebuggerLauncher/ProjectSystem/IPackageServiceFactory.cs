// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using RemoteDebuggerLauncher.RemoteOperations;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface defining a factory for worker services.
   /// </summary>
   internal interface IPackageServiceFactory
   {
      /// <summary>
      /// Gets the name of the active launch profile.
      /// </summary>
      string ActiveLaunchProfileName { get; }

      /// <summary>
      /// Gets the configuration aggregator.
      /// </summary>
      ConfigurationAggregator Configuration { get; }

      /// <summary>
      /// Gets the configured VS ouput pane writer.
      /// </summary>
      IOutputPaneWriterService OutputPane { get; }

      /// <summary>
      /// Gets the SSH deploy service.
      /// </summary>
      /// <param name="useWaitDialog"><c>true</c> to use the VS wait dialog, else <c>false</c>.</param>
      /// <returns>A <see cref="ISecureShellDeployService"/> instance.</returns>
      Task<ISecureShellDeployService> GetDeployServiceAsync(bool useWaitDialog);

      /// <summary>
      /// Gets the .NET publish service responsible to run dotnet publish if required.
      /// </summary>
      /// <param name="useWaitDialog"><c>true</c> to use the VS wait dialog, else <c>false</c>.</param>
      /// <returns>A <see cref="ISecureShellDeployService"/> instance.</returns>
      Task<IDotnetPublishService> GetPublishServiceAsync(bool useWaitDialog);

      Task<ISecureShellRemoteOperationsService> GetSecureShellRemoteOperationsAsync();

      Task<IStatusbarService> GetStatusbarServiceAsync();
   }
}
