// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
      /// The following configuration provides are queried, first match wins
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
      /// The following configuration provides are queried, first match wins
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
      /// Queries the host SSH port number to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see langword="int"/> holding the port; the default port (22) if unconfigured.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - built-in default
      /// </remarks>
      public int QueryHostPort()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.hostPortProperty, out var settingsValue))
         {
            if (settingsValue is int profileHostPort)
            {
               // Launch profile has a host port specified => use it
               return profileHostPort;
            }
         }

         // use options value or default
         return optionsPageAccessor.QueryHostPort();
      }

      /// <summary>
      /// Queries the private key to be used to establish a connection to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the private key file path; an empty string if no key is configured.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
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
               return Environment.ExpandEnvironmentVariables(profilePrivateKey);
            }
         }

         var optionsPrivateKey = optionsPageAccessor.QueryPrivateKeyFilePath();
         if (!string.IsNullOrEmpty(optionsPrivateKey))
         {
            // Options has a user name specified => use it
            return Environment.ExpandEnvironmentVariables(optionsPrivateKey);
         }

         // No private key available, rely on defaults
         return string.Empty;
      }

      /// <summary>
      /// Queries the folder path where the .NET framework is installed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - Built-in defaults (~/.dotnet)
      /// </remarks>
      public string QueryDotNetInstallFolderPath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.dotNetInstallFolderPathProperty, out var settingsValue))
         {
            if (settingsValue is string profileDotNetInstallFolerPath && !string.IsNullOrEmpty(profileDotNetInstallFolerPath))
            {
               // Launch profile has path specified => use it
               return profileDotNetInstallFolerPath;
            }
         }

         var optionsDotNetInstallFolderPath = optionsPageAccessor.QueryDotNetInstallFolderPath();
         if (!string.IsNullOrEmpty(optionsDotNetInstallFolderPath))
         {
            // Options has path specified => use it
            return optionsDotNetInstallFolderPath;
         }

         // No path configured, relay on built-in defaults
         return PackageConstants.Options.DefaultValueDotNetInstallFolderPath;
      }

      /// <summary>
      /// Queries the path where the VS Code Debugger is installed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - Built-in defaults
      /// </remarks>
      public string QueryDebuggerInstallFolderPath()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.debuggerInstallFolderPathProperty, out var settingsValue))
         {
            if (settingsValue is string profileDebuggerInstallFolderPath && !string.IsNullOrEmpty(profileDebuggerInstallFolderPath))
            {
               // Launch profile has path specified => use it
               return profileDebuggerInstallFolderPath;
            }
         }

         var optionsDebuggerInstallFolderPath = optionsPageAccessor.QueryDebuggerInstallFolderPath();
         if (!string.IsNullOrEmpty(optionsDebuggerInstallFolderPath))
         {
            // Options has path specified => use it
            return optionsDebuggerInstallFolderPath;
         }

         // No path configured, relay on built-in defaults
         return PackageConstants.Options.DefaultValueDotNetInstallFolderPath;
      }

      /// <summary>
      /// Queries the path where the application gets deployed on the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the path.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - Built-in defaults
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

         // No path configured, relay on built-in defaults
         return PackageConstants.Options.DefaultValueAppFolderPath;
      }

      /// <summary>
      /// Queries the command line arguments to be passed to the starting executable.
      /// </summary>
      /// <returns>A <see langword="string"/> holding the arguments.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// </remarks>
      public string QueryCommandLineArguments()
      {
         var profileCommandLineArgs = launchProfile.CommandLineArgs;

         if (!string.IsNullOrEmpty(profileCommandLineArgs))
         {
            return profileCommandLineArgs;
         }

         // No args configured
         return string.Empty;
      }

      /// <summary>
      /// Queries the environment variables to be passed to the starting executable.
      /// </summary>
      /// <returns>A <see cref="IImmutableDictionary"/> holding the variables.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// </remarks>
      public IImmutableDictionary<string, string> QueryEnvironmentVariables() => launchProfile.EnvironmentVariables;

      /// <summary>
      /// Queries the flag whether to install the VS code debugger on deploy.
      /// </summary>
      /// <returns>A <see langword="bool"/><c>true</c> if debugger should be installed; else <c>false</c>.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - built-in default (false)
      /// </remarks>
      public bool QueryInstallDebuggerOnDeploy()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.installDebuggerOnDeployProperty, out var settingsValue))
         {
            if (settingsValue is bool installDebuggerOnDeploy)
            {
               // Launch profile value => use if
               return installDebuggerOnDeploy;
            }
         }

         //rely on built-in default
         return false;
      }

      /// <summary>
      /// Queries the flag whether to publish the application on deploy.
      /// </summary>
      /// <returns>A <see langword="bool"/><c>true</c> to deploy published output;<c>false</c> to deploy build output.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - built-in default (false)
      /// </remarks>
      public bool QueryPublishOnDeploy()
      {
         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.publishOnDeployProperty, out var settingsValue))
         {
            if (settingsValue is bool publishOnDeploy)
            {
               // Launch profile value => use if
               return publishOnDeploy;
            }
         }

         // use the default in options 
         var optionsPublishOnDeploy = optionsPageAccessor.QueryPublishOnDeploy();
         return optionsPublishOnDeploy;
      }
   }
}
