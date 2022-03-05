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
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   internal class SecureShellSessionSettings
   {
      private SecureShellSessionSettings(ConfigurationAggregator configurationAggregator)
      {
         HostName = configurationAggregator.QueryHostName();
         UserName = configurationAggregator.QueryUserName();
      }

      public string HostName { get; private set; }

      public string UserName { get; private set; }

      public static SecureShellSessionSettings Create(ConfigurationAggregator configurationAggregator)
      {
         return new SecureShellSessionSettings(configurationAggregator);
      }

   }
}
