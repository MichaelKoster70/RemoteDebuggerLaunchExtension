// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Container for Secure Shell sessions holding all required settings,
   /// </summary>
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

      public static SecureShellSessionSettings Create(ConfigurationAggregator configurationAggregator)
      {
         return new SecureShellSessionSettings(configurationAggregator);
      }
   }
}
