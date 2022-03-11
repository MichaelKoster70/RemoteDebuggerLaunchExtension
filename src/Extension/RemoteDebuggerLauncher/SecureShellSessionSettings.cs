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
         PrivateKeyFile = configurationAggregator.QueryPrivateKeyFilePath();
      }

      public string HostName { get; private set; }

      public string UserName { get; private set; }

      public string PrivateKeyFile { get; private set; }

      public AuthenticationKind Authentication => AuthenticationKind.PrivateKey;

      public static SecureShellSessionSettings Create(ConfigurationAggregator configurationAggregator)
      {
         return new SecureShellSessionSettings(configurationAggregator);
      }
   }
}
