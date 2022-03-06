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
   /// <summary>
   /// This class is responsible for aggregating the configuration options from launch profile settings and VS tools options.
   /// </summary>
   internal class ConfigurationAggregator
   {
      private readonly ILaunchProfile launchProfile;
      private readonly IOptionsPageAccessor optionsPageAccessor;

      private ConfigurationAggregator(ILaunchProfile launchProfile, IOptionsPageAccessor optionsPageAccessor)
      {
         ThrowIf.ArgumentNull(launchProfile, nameof(launchProfile));
         ThrowIf.ArgumentNull(optionsPageAccessor, nameof(optionsPageAccessor));

         this.launchProfile = launchProfile;
         this.optionsPageAccessor = optionsPageAccessor;
      }

      /// <summary>
      /// Creates a aggregator instance.
      /// </summary>
      /// <param name="launchProfile">The launch profile.</param>
      /// <param name="optionsPageAccessor">The options page accessor.</param>
      /// <returns>An <see cref="ConfigurationAggregator"/> instance.</returns>
      public static ConfigurationAggregator Create(ILaunchProfile launchProfile, IOptionsPageAccessor optionsPageAccessor)
      {
         return new ConfigurationAggregator(launchProfile, optionsPageAccessor);
      }

      /// <summary>
      /// Queries the user name to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the user name; an empty string if no name is configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a user name, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public string QueryUserName()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.userNameProperty, out var settingsValue))
         {
            if (settingsValue is string profileUserName && !string.IsNullOrEmpty(profileUserName))
            {
               // Launch profile has a user name specified => use it
               return profileUserName;
            }
         }

         var optionsUserName = optionsPageAccessor.QueryUserName();
         if (!string.IsNullOrEmpty(optionsUserName))
         {
            // Options has a user name specified => use it
            return optionsUserName;
         }

         // No User Name available
         return string.Empty;
      }

      /// <summary>
      /// Queries the host name to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the host name; an empty string if no name is configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a host name, first match wins
      /// - selected launch profile
      /// </remarks>
      public string QueryHostName()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.hostNameProperty, out var settingsValue))
         {
            if (settingsValue is string profileHostName && !string.IsNullOrEmpty(profileHostName))
            {
               // Launch profile has a host name specified => use it
               return profileHostName;
            }
         }

         // No Host Name available
         return string.Empty;
      }

      /// <summary>
      /// Queries the private key to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the private key file path; an empty string if no key is configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a private key, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public string QueryPrivateKeyFilePath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.privateKeyProperty, out var settingsValue))
         {
            if (settingsValue is string profilePrivateKey && !string.IsNullOrEmpty(profilePrivateKey))
            {
               // Launch profile has a key file  specified => use it
               return profilePrivateKey;
            }
         }

         var optionsPrivateKey = optionsPageAccessor.QueryPrivateKeyFilePath();
         if (!string.IsNullOrEmpty(optionsPrivateKey))
         {
            // Options has a user name specified => use it
            return optionsPrivateKey;
         }

         // No private key available
         return string.Empty;
      }

      /// <summary>
      /// Queries the adapter provider to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see cref="AdapterProviderKind"/> instance.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a provider, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public AdapterProviderKind QueryAdapterProvider()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.sshProviderProperty, out var settingsValue))
         {
            if (settingsValue is string profileProvider)
            {
               switch (profileProvider)
               {
                  case SecureShellRemoteLaunchProfile.sshProviderValues.windowsSSH:
                     return AdapterProviderKind.WindowsSSH;
                  case SecureShellRemoteLaunchProfile.sshProviderValues.putty:
                     return AdapterProviderKind.PuTTY;
                  default:
                     break;
               }
            }
         }

         var optionsProvider = optionsPageAccessor.QueryAdapterProvider();

         switch (optionsProvider)
         {
            case AdapterProviderKind.WindowsSSH:
               return optionsProvider;
            case AdapterProviderKind.PuTTY:
               return optionsProvider;
            default:
               return AdapterProviderKind.WindowsSSH;
         }
      }

      /// <summary>
      /// Queries the path where .NET 'dotnet' is installed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path; an empty string not configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a path, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public string QueryDotNetInstallPath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.dotNetInstallPathProperty, out var settingsValue))
         {
            if (settingsValue is string profileDotNetInstallPath && !string.IsNullOrEmpty(profileDotNetInstallPath))
            {
               // Launch profile has path specified => use it
               return profileDotNetInstallPath;
            }
         }

         var optionsDotNetInstallPath = optionsPageAccessor.QueryDotNetInstallPath();
         if (!string.IsNullOrEmpty(optionsDotNetInstallPath))
         {
            // Options has path specified => use it
            return optionsDotNetInstallPath;
         }

         // No path key available
         return string.Empty;
      }

      /// <summary>
      /// Queries the path where the VS Code Debugger is installed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path; an empty string not configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a path, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public string QueryDebuggerInstallPath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.debuggerInstallPathProperty, out var settingsValue))
         {
            if (settingsValue is string profileDebuggerInstallPath && !string.IsNullOrEmpty(profileDebuggerInstallPath))
            {
               // Launch profile has path specified => use it
               return profileDebuggerInstallPath;
            }
         }

         var optionsDebuggerInstallPath = optionsPageAccessor.QueryDebuggerInstallPath();
         if (!string.IsNullOrEmpty(optionsDebuggerInstallPath))
         {
            // Options has path specified => use it
            return optionsDebuggerInstallPath;
         }

         // No path key available
         return string.Empty;
      }

      /// <summary>
      /// Queries the path where the VS Code Debugger is installed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path; an empty string not configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried for a private key, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// </remarks>
      public string QueryAppFolderPath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.appFolderPathProperty, out var settingsValue))
         {
            if (settingsValue is string profileAppFolderPath && !string.IsNullOrEmpty(profileAppFolderPath))
            {
               // Launch profile has path specified => use it
               return profileAppFolderPath;
            }
         }

         var optionsAppFolderPath = optionsPageAccessor.QueryAppFolderPath();
         if (!string.IsNullOrEmpty(optionsAppFolderPath))
         {
            // Options has path specified => use it
            return optionsAppFolderPath;
         }

         // No path key available
         return string.Empty;
      }
   }
}
