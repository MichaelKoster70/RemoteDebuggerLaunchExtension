// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Container for Secure Shell settings.
   /// </summary>
   internal class SecureShellSessionSettings
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionSettings"/> class initialized from the supplied config aggreegator.
      /// </summary>
      /// <param name="configurationAggregator">The aggregator to read settings from.</param>
      private SecureShellSessionSettings(ConfigurationAggregator configurationAggregator)
      {
         HostName = configurationAggregator.QueryHostName();
         HostPort = configurationAggregator.QueryHostPort();
         UserName = configurationAggregator.QueryUserName();
         PrivateKeyFile = configurationAggregator.QueryPrivateKeyFilePath();
      }

      /// <summary>
      /// Gets the host name of the target device.
      /// </summary>
      public string HostName { get; private set; }

      /// <summary>
      /// Gets the host port of the target device.
      /// </summary>
      public int HostPort { get; private set; }

      /// <summary>
      /// Gets a value whether <see cref="HostPort"/> ist the default port.
      /// </summary>
      public bool IsHostPortDefault => HostPort == PackageConstants.Options.DefaultValueSecureShellHostPort;

      /// <summary>
      /// Gets the user name.
      /// </summary>
      public string UserName { get; private set; }

      /// <summary>
      /// Gets the private key file.
      /// </summary>
      public string PrivateKeyFile { get; private set; }

      /// <summary>
      /// Create a new instance of the <see cref="SecureShellSessionSettings"/> class initialized from the supplied config aggreegator.
      /// </summary>
      /// <param name="configurationAggregator">The aggregator to read settings from.</param>
      /// <returns>The <see cref="SecureShellSessionSettings"/> instance.</returns>
      public static SecureShellSessionSettings Create(ConfigurationAggregator configurationAggregator)
      {
         return new SecureShellSessionSettings(configurationAggregator);
      }
   }
}
