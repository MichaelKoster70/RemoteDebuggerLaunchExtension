// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class providing covenience methods to get local package services.
   /// </summary>
   internal static class ServiceExtensions
   {
      /// <summary>
      /// Gets the Status Bar service interface from a service provider.
      /// </summary>
      /// <param name="asyncServieProvier">The service provider to query.</param>
      /// <returns>The <see cref="IStatusbarService"/> service interface. Never null.</returns>
      /// <exception cref="ServiceUnavailableException">The service could not be acquired.<exception>
      public static Task<IStatusbarService> GetStatusbarServiceAsync(this IAsyncServiceProvider asyncServieProvier)
      {
         return asyncServieProvier.GetServiceAsync<SStatusbarService, IStatusbarService>();
      }

      /// <summary>
      /// Gets the Logger service interface from a service provider.
      /// </summary>
      /// <param name="asyncServieProvier">The service provider to query.</param>
      /// <returns>The <see cref="ILoggerService"/> service interface. Never null.</returns>
      /// <exception cref="ServiceUnavailableException">The service could not be acquired.<exception>
      public static Task<ILoggerService> GetLoggerServiceAsync(this IAsyncServiceProvider asyncServieProvier)
      {
         return asyncServieProvier.GetServiceAsync<SLoggerService, ILoggerService>();
      }
   }
}
