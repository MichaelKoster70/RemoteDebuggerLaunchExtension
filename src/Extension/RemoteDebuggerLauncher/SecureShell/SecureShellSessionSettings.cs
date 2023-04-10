// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net;

namespace RemoteDebuggerLauncher.SecureShell
{
   /// <summary>
   /// Container for Secure Shell settings.
   /// </summary>
   internal class SecureShellSessionSettings
   {
      private readonly bool forceIPv4;

      /// <summary>
      /// Initializes a new instance of the <see cref="SecureShellSessionSettings"/> class initialized from the supplied config aggregator.
      /// </summary>
      /// <param name="configurationAggregator">The aggregator to read settings from.</param>
      private SecureShellSessionSettings(ConfigurationAggregator configurationAggregator)
      {
         HostName = configurationAggregator.QueryHostName();
         HostPort = configurationAggregator.QueryHostPort();
         UserName = configurationAggregator.QueryUserName();
         PrivateKeyFile = configurationAggregator.QueryPrivateKeyFilePath(true);
         forceIPv4 = configurationAggregator.QueryForceIPv4();
      }

      /// <summary>
      /// Gets the host name of the target device.
      /// </summary>
      public string HostName { get; }

      /// <summary>
      /// Gets the IPv4 address of the target device.
      /// </summary>
      public string HostNameIPv4
      {
         get
         {
            if (forceIPv4)
            {
               return Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString() ?? HostName;
            }
            else
            {
               return HostName;
            }
         }
      }

      /// <summary>
      /// Gets the host port of the target device.
      /// </summary>
      public int HostPort { get; }

      /// <summary>
      /// Gets a value whether <see cref="HostPort"/> ist the default port.
      /// </summary>
      public bool IsHostPortDefault => HostPort == PackageConstants.Options.DefaultValueSecureShellHostPort;

      /// <summary>
      /// Gets the user name.
      /// </summary>
      public string UserName { get; }

      /// <summary>
      /// Gets the private key file.
      /// </summary>
      public string PrivateKeyFile { get; }

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
