// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.Net;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using RemoteDebuggerLauncher.Shared;

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
      /// Gets the name of the underlying launch profile
      /// </summary>
      /// <value>A <see langword="string"/>holding the name.</value>
      public string LaunchProfileName => launchProfile.Name;

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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.userNameProperty, out string profileUserName, HasValue))
         {
            // Launch profile has a user name specified => use it
            return profileUserName;
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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.hostNameProperty, out string profileHostName, HasValue))
         {
            // Launch profile has a host name specified => use it
            return profileHostName;
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
         // number validator function
         bool IsValidHostPort(int portNumber) =>  IPEndPoint.MinPort < portNumber && portNumber <= IPEndPoint.MaxPort;

         if (launchProfile.OtherSettings.TryGetValue(SecureShellRemoteLaunchProfile.hostPortProperty, out var settingsValue))
         {
            if (settingsValue is string profileHostPort && !string.IsNullOrEmpty(profileHostPort) 
               && int.TryParse(profileHostPort, out int profileHostPortAsInteger) && IsValidHostPort(profileHostPortAsInteger))
            {
               // Launch profile has a host port specified => use it
               return profileHostPortAsInteger;
            }

            if (settingsValue is int profileHostPortNumber && IsValidHostPort(profileHostPortNumber))
            {
               // Launch profile has a host port specified => use it
               return profileHostPortNumber;
            }

            // ignoring the invalid value specified
         }

         // use options value or default
         return optionsPageAccessor.QueryHostPort();
      }

      /// <summary>
      /// Queries the value whether to force IPv4 connections from the device option page.
      /// </summary>
      /// <returns>A <see langword="bool"/> holding the value, <c>false</c> if not configured.</returns>
      public bool QueryForceIPv4()
      {
         return optionsPageAccessor.QueryForceIPv4();
      }


      /// <summary>
      /// Queries the transport mode on how to transfer assets to the remote device.
      /// </summary>
      /// <returns></returns>
      public TransferMode QueryTransferMode()
      {
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.deployTransferModeProperty, out string transferModeText) 
            && Enum.TryParse<TransferMode>(transferModeText, out TransferMode transferMode))
         {
            // Launch profile value => use it
            return transferMode;
         }

         // use options value or default
         return optionsPageAccessor.QueryDeployTransferMode();
      }

      public bool QueryDeployClean()
      {
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.deployCleanProperty, out bool deployClean))
         {
            // Launch profile value => use it
            return deployClean;
         }

         // use options value or default
         return optionsPageAccessor.QueryDeployClean();
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
      public string QueryPrivateKeyFilePath(bool provideDefault = false)
      {
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.privateKeyProperty, out string profilePrivateKey, HasValue))
         {
            // Launch profile has a key file  specified => use it
            return Environment.ExpandEnvironmentVariables(profilePrivateKey);
         }

         var optionsPrivateKey = optionsPageAccessor.QueryPrivateKeyFilePath();
         if (!string.IsNullOrEmpty(optionsPrivateKey))
         {
            // Options has a user name specified => use it
            return Environment.ExpandEnvironmentVariables(optionsPrivateKey);
         }

         // No private key available, rely on defaults
         return provideDefault ? Environment.ExpandEnvironmentVariables(PackageConstants.Options.DefaultValuePrivateKey) : string.Empty;
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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.dotNetInstallFolderPathProperty, out string profileDotNetInstallFolderPath, HasValue))
         {
            // Launch profile has path specified => use it
            return profileDotNetInstallFolderPath;
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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.debuggerInstallFolderPathProperty, out string profileDebuggerInstallFolderPath, HasValue))
         {
            // Launch profile has path specified => use it
            return profileDebuggerInstallFolderPath;
         }

         var optionsDebuggerInstallFolderPath = optionsPageAccessor.QueryDebuggerInstallFolderPath();
         if (!string.IsNullOrEmpty(optionsDebuggerInstallFolderPath))
         {
            // Options has path specified => use it
            return optionsDebuggerInstallFolderPath;
         }

         // No path configured, relay on built-in defaults
         return PackageConstants.Options.DefaultValueDebuggerInstallFolderPath;
      }

      public string QueryToolsInstallFolderPath()
      {
         var optionsToolsInstallFolderPath = optionsPageAccessor.QueryToolsInstallFolderPath();
         if (!string.IsNullOrEmpty(optionsToolsInstallFolderPath))
         {
            // Options has path specified => use it
            return optionsToolsInstallFolderPath;
         }

         // No path configured, relay on built-in defaults
         return PackageConstants.Options.DefaultValueToolsInstallFolderPath;
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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.appFolderPathProperty, out string profileAppFolderPath, HasValue))
         {
            // Launch profile has path specified => use it
            return profileAppFolderPath;
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
      /// Queries the flag whether to install the VS code debugger when start debugging.
      /// </summary>
      /// <returns>A <see langword="bool"/><c>true</c> if debugger should be installed; else <c>false</c>.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - built-in default (false)
      /// </remarks>
      public bool QueryInstallDebuggerOnStartDebugging()
      {
         return GetOtherSetting(SecureShellRemoteLaunchProfile.installDebuggerOnStartDebuggingProperty, false);
      }

      /// <summary>
      /// Queries the flag whether to deploy before start debugging.
      /// </summary>
      /// <returns>A <see langword="bool"/><c>true</c> if the app should be deployed; else <c>false</c>.</returns>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - built-in default (false)
      /// </remarks>
      public bool QueryDeployOnStartDebugging()
      {
         return GetOtherSetting(SecureShellRemoteLaunchProfile.deployOnStartDebuggingProperty, false);
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
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.publishOnDeployProperty, out bool publishOnDeploy))
         {
            // Launch profile value => use it
            return publishOnDeploy;
         }

         // use the default in options 
         var optionsPublishOnDeploy = optionsPageAccessor.QueryPublishOnDeploy();
         return optionsPublishOnDeploy;
      }

      /// <summary>
      /// Queries the publishing mode.
      /// </summary>
      /// <returns>One of the <see see="PublishMode"/> values</returns>
      /// <summary>
      /// <remarks>
      /// The following configuration provides are queried, first match wins
      /// - selected launch profile
      /// - Tools/Options settings
      /// - built-in default (false)
      /// </remarks>
      public PublishMode QueryPublishMode()
      {
         if (GetOtherSetting(SecureShellRemoteLaunchProfile.publishModeProperty, out string publishModeText)
            && Enum.TryParse<PublishMode>(publishModeText, out PublishMode publishMode))
         {
            // Launch profile value => use it
            return publishMode;
         }

         // use the default in options 
         var optionsPublishMode = optionsPageAccessor.QueryPublishMode();
         return optionsPublishMode;
      }

      /// <summary>
      /// Queries the flag whether to launch the default browser.
      /// </summary>
      /// <value>A <see langword="bool"/><c>true</c> to launch the browser; else <c>false</c>.</value>
      public bool QueryLaunchBrowser() => launchProfile.LaunchBrowser;

      /// <summary>
      /// Queries the browser launch URI, if specified.
      /// </summary>
      /// <returns>The URI if valid; else <c>null</c></returns>
      public Uri QueryBrowserLaunchUri()
      {
         try
         {
            return !string.IsNullOrWhiteSpace(launchProfile.LaunchUrl) ? new Uri(launchProfile.LaunchUrl) : null;
         }
         catch (UriFormatException)
         {
            return null;
         }
      }

      /// <summary>
      /// Queries the browser inspect URI, if specified.
      /// </summary>
      /// <returns>The URI if valid; else <c>null</c></returns>
      public string QueryInspectUri() => GetOtherSetting<string>("inspectUri");

      /// <summary>
      /// Queries the additional files to be deployed to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> containing additional file mappings in format 'source1|target1;source2|target2'.</returns>
      /// <remarks>
      /// The following configuration providers are queried, first match wins:
      /// - selected launch profile
      /// </remarks>
      public string QueryAdditionalFiles()
      {
         return GetOtherSetting<string>("additionalFiles") ?? string.Empty;
      }

      /// <summary>
      /// Queries the additional folders to be deployed to the remote device.
      /// </summary>
      /// <returns>A <see langword="string"/> containing additional folder mappings in format 'source1|target1;source2|target2'.</returns>
      /// <remarks>
      /// The following configuration providers are queried, first match wins:
      /// - selected launch profile
      /// </remarks>
      public string QueryAdditionalFolders()
      {
         return GetOtherSetting<string>("additionalFolders") ?? string.Empty;
      }

      /// <summary>
      ///  Gets the value of the specified property from the launch profile's other settings.
      /// </summary>
      /// <typeparam name="T">The parameter type.</typeparam>
      /// <param name="propertyName">The name of the property to get.</param>
      /// <param name="defaultValue">The default value to return, if the </param>
      /// <returns>The property value, default if not found</returns>
      private T GetOtherSetting<T>(string propertyName, T defaultValue = default)
      {
         return launchProfile.OtherSettings != null && launchProfile.OtherSettings.TryGetValue(propertyName, out object value) && value is T targetValue ? targetValue : defaultValue;
      }

      private bool GetOtherSetting<T>(string propertyName, out T targetValue, Predicate<T> predicate)
      {
         return GetOtherSetting(propertyName, out targetValue) && predicate(targetValue);
      }

      /// <summary>
      ///  Gets the value of the specified property from the launch profile's other settings.
      /// </summary>
      /// <typeparam name="T">The parameter type.</typeparam>
      /// <param name="propertyName">The name of the property to get.</param>
      /// <param name="targetValue">The value of the property.</param>
      /// <returns><c>true</c> if available in the launch profile, <c>false</c> otherwise.</returns>
      private bool GetOtherSetting<T>(string propertyName, out T targetValue)
      {
         if (launchProfile.OtherSettings != null && launchProfile.OtherSettings.TryGetValue(propertyName, out object value) && value is T target)
         {
            targetValue = target;
            return true;
         }

         targetValue = default;
         return false;
      }

      /// <summary>
      /// Predicate validating that the supplied <see langword=""="string"/> has a non empty value.
      /// </summary>
      /// <param name="text">The text to validate.</param>
      /// <returns><c>true</c> if not empty, <c>false</c> otherwise.</returns>
      private static bool HasValue(string text)
      {
         return !string.IsNullOrEmpty(text);
      }
   }
}
