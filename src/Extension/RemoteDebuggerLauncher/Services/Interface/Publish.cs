// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.ProjectSystem;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing <see cref="IPublishService"/> factorties.
   /// </summary>
   internal static class Publish
   {
      /// <summary>
      /// Creates a <see cref="ISecureShellRemoteOperationsService" /> using the supplied project
      /// </summary>
      /// <param name="configuredProject">The configured project to publish.</param>
      /// <param name="logger">The logger instance to use.</param>
      /// <param name="statusbar">Optional statusbar service to report progress.</param>
      /// <returns>A <see cref="IPublishService" /> instance.</returns>
      /// <exception cref="ArgumentNullException">configuredProject is null.</exception>
      /// <exception cref="ArgumentNullException">logger is null.</exception>
      public static IPublishService Create(ConfiguredProject configuredProject, ILoggerService logger, IStatusbarService statusbar = null)
      {
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));

         return new PublishService(configuredProject, logger, statusbar);
      }
   }
}
