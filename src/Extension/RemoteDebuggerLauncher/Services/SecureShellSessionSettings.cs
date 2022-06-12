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
         Authentication = configurationAggregator.QueryAuthenication();
         if (Authentication == AuthenticationKind.PrivateKey)
         {
            PrivateKeyFile = configurationAggregator.QueryPrivateKeyFilePath();
         }
         else
         {
            Password = configurationAggregator.QueryPassword();
         }
      }

      public string HostName { get; private set; }

      public string UserName { get; private set; }

      public AuthenticationKind Authentication { get; private set; }

      public string PrivateKeyFile { get; private set; } = string.Empty;

      public string Password { get; private set; } = string.Empty;

      public static SecureShellSessionSettings Create(ConfigurationAggregator configurationAggregator)
      {
         return new SecureShellSessionSettings(configurationAggregator);
      }
   }
}
