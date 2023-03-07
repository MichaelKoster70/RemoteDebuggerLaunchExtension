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
   /// Interface defining the service responsible for producing publishable build output.
   /// </summary>
   internal interface IDotnetPublishService
   {
      /// <summary>
      /// Gets a value whether the publish result is considered up to date.
      /// </summary>
      /// <returns><c>true</c> if up-to date; else <c>false</c></returns>
      /// <remarks>Not yet implemented</remarks>
      Task<bool> IsUpToDateAsync();

      /// <summary>
      /// Starts the publish process.
      /// </summary>
      /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
      /// <exception cref="RemoteDebuggerLauncherException">If publish fails.</exception>
      Task StartAsync();

      /// <summary>
      /// Returns the output directory of the project.
      /// </summary>
      /// <returns>A <see cref="Task{string}"/> representing the asynchronous operation: the project output directory.</returns>
      Task<string> GetOutputDirectoryPathAsync();
   }
}
