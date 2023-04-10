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
   /// Data container holding configuration data for the <see cref="ISecureShellKeySetupService"/>
   /// </summary>
   internal class SecureShellKeySetupSettings
   {
      private readonly bool forceIPv4;
      public SecureShellKeySetupSettings(SetupSshViewModel viewModel)
      {
         HostName = viewModel.HostName;
         HostPort = viewModel.HostPort;
         UserName = viewModel.Username;
         Password = viewModel.Password;
         PublicKeyFile= viewModel.PublicKeyFile;
         PrivateKeyFile = viewModel.PrivateKeyFile;
         forceIPv4 = viewModel.ForceIPv4;
      }

      /// <summary>
      /// Gets the host name of the target device.
      /// </summary>
      public string HostName { get; }

      /// <summary>
      /// Gets the IPv4 address of the target device.
      /// </summary>
      public string HostNameIPv4 => forceIPv4 ? Dns.GetHostEntry(HostName).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? HostName : HostName;

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
      /// Gets the password.
      /// </summary>
      public string Password { get; }

      /// <summary>
      /// Gets the public key file.
      /// </summary>
      public string PublicKeyFile { get; }

      /// <summary>
      /// Gets the private key file.
      /// </summary>
      public string PrivateKeyFile { get; }
   }
}
