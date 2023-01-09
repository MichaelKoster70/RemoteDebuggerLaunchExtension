// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class to generate the JSON object required to configure the VS Code Debugger Adapter.
   /// </summary>
   /// <remarks>
   /// The launch.json configuration property described in https://github.com/OmniSharp/omnisharp-vscode/blob/master/debugger-launchjson.md behave as followed
   /// - "justMyCode" - will be added by Visual Studio based on the configured option
   /// - "stopAtEntry" - is handled by Visual Studio when pressing F11 (step-into)
   /// </remarks>
   internal static class AdapterLaunchConfiguration
   {
      /// <summary>Holds a environment variable name value pair.</summary>
      internal class EnvironmentEntry
      {
         [JsonProperty("name")]
         public string Name { get; set; }

         [JsonProperty("value")]
         public string Value { get; set; }
      }

      /// <summary>Holds a environment variable name value pair.</summary>
      internal class LaunchConfiguration
      {
         [JsonProperty("version")]
         public string Version => "0.2.0";

         [JsonProperty("name")]
         public string Name { get; set; }

         [JsonProperty("request")]
         public string Request => "launch";

         [JsonProperty("type")]
         public string Type => "coreclr";

         [JsonProperty("$adapter")]
         public string Adapter { get; set; }

         [JsonProperty("$adapterArgs")]
         public string AdapterArgs { get; set; }

         /// <summary>The program to debug.</summary>
         /// <remarks>Must be the path to dotnet(.exe) for framework dependant programs</remarks>
         [JsonProperty("program")]
         public string Program { get; set; }

         /// <summary>Command line arguments, the first being the .NET assembly to launch.</summary>
         [JsonProperty("args")]
         public List<string> Args { get; } = new List<string>();

         [JsonProperty("cwd")]
         public string CurrentWorkingDirectory { get; set; }

         /// <summary>The environment variable to pass to the program.</summary>
         [JsonProperty("environment")]
         public IList<EnvironmentEntry> Environment { get; } = new List<EnvironmentEntry>();

         /// <summary>The VS Code console option.</summary>
         /// <remarks>Have no effect in a remote debugger scenario.</remarks>
         [JsonProperty("console")]
         public string Console => "internalConsole";

         /// <summary>Additional settings</summary>
         [JsonExtensionData]
         public Dictionary<string, JToken> ConfigurationProperties { get; } = new Dictionary<string, JToken>();
      }

      /// <summary>
      /// Create a launch configuration JSON for a .NET framework dependant application.
      /// </summary>
      /// <param name="configurationAggregator">The configuration aggregator to read the config from.</param>
      /// <param name="configuredProject">The CPS project to get the configuration for.</param>
      /// <param name="logger">The logger for diagnostics logging.</param>
      /// <param name="remoteOperations">The remote operation service to use.</param>
      /// <returns>A JSON configuration object.</returns>
      public static async Task<string> CreateFrameworkDependantAsync(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject, ILoggerService logger, ISecureShellRemoteOperationsService remoteOperations)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));

         var program = UnixPath.Combine(configurationAggregator.QueryDotNetInstallFolderPath(), PackageConstants.Dotnet.BinaryName);
         var appFolderPath = configurationAggregator.QueryAppFolderPath();
         var assemblyFileName = await configuredProject.GetAssemblyFileNameAsync();
         var assemblyFileDirectory = ".";
         var workingDirectory = appFolderPath;

         // check whether the path values should be normalized
         var shouldNormalizeProgram = UnixPath.ShouldBeNormalized(program);
         var shouldNormalizeAppFolderPath = UnixPath.ShouldBeNormalized(appFolderPath);
         if (shouldNormalizeProgram || shouldNormalizeAppFolderPath)
         {
            var homeDirectory = await remoteOperations.QueryUserHomeDirectoryAsync();
            if (!string.IsNullOrEmpty(homeDirectory))
            {
               // we have a home directory
               program = UnixPath.Normalize(program, homeDirectory);
               appFolderPath = UnixPath.Normalize(appFolderPath, homeDirectory);
               assemblyFileDirectory = appFolderPath;
               workingDirectory = appFolderPath;
            }
            else
            {
               // no home directory found
               // program: keep as configured
               // appFolderPath: keep as configured
               // assemblyFileDirectory: keep current directory
               // workingDirectory: keep appFolderPath
            }
         }
         else
         {
            // we only use absolut pathes
            // program: keep as configured
            // appFolderPath: keep as configured
            assemblyFileDirectory = appFolderPath;
            workingDirectory = appFolderPath;
         }

         var config = CreateAndSetAdapter(configurationAggregator);
         config.Name = ".NET Core Launch - Framework dependant";
         config.Program = program;
         config.Args.Add($"{assemblyFileDirectory}/{assemblyFileName}");
         config.CurrentWorkingDirectory = workingDirectory;
         config.AppendCommandLineArguments(configurationAggregator);
         config.AppendEnvironmentVariables(configurationAggregator);

         var launchConfigurationJson = JsonConvert.SerializeObject(config);

         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
         logger.WriteLine($"Options: {launchConfigurationJson}");
         return launchConfigurationJson;
      }

      public static async Task<string> CreateSelfContainedAsync(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));

         var program = UnixPath.Combine(configurationAggregator.QueryDotNetInstallFolderPath(), PackageConstants.Dotnet.BinaryName);
         var appFolderPath = configurationAggregator.QueryAppFolderPath();
         var assemblyFileName = await configuredProject.GetAssemblyFileNameAsync();

         var config = CreateAndSetAdapter(configurationAggregator);
         config.Name = ".NET Core Launch - Self contained";
         config.Program = program;
         config.Args.Add($"./{assemblyFileName}");
         config.CurrentWorkingDirectory = appFolderPath;
         config.AppendCommandLineArguments(configurationAggregator);
         config.AppendEnvironmentVariables(configurationAggregator);

         return JsonConvert.SerializeObject(config);
      }

      private static LaunchConfiguration CreateAndSetAdapter(ConfigurationAggregator configurationAggregator)
      {
         // Collect the configurable options
         var hostName = configurationAggregator.QueryHostName();
         var userName = configurationAggregator.QueryUserName();
         var vsdbgPath = UnixPath.Combine(configurationAggregator.QueryDebuggerInstallFolderPath(), PackageConstants.Debugger.BinaryName);

         // assemble the adapter and adapterArg values
         string adapter = PackageConstants.DebugLaunchSettings.Options.AdapterNameWindowsSSH;
         string adapterArgs = string.Empty;

         var privateKey = configurationAggregator.QueryPrivateKeyFilePath();
         adapterArgs += !string.IsNullOrEmpty(privateKey) ? $"-i {privateKey} " : string.Empty;
         adapterArgs += $"{userName}@{hostName} {vsdbgPath} --interpreter=vscode";

         var config = new LaunchConfiguration()
         {
            Adapter = adapter,
            AdapterArgs = adapterArgs,
         };

         return config;
      }

      private static void AppendCommandLineArguments(this LaunchConfiguration config, ConfigurationAggregator configurationAggregator)
      {
         var commandLineArgs = configurationAggregator.QueryCommandLineArguments();

         if (!string.IsNullOrEmpty(commandLineArgs))
         {
            var args = commandLineArgs.Split(' ');
            Array.ForEach(args, (arg) => config.Args.Add(arg));
         }
      }

      private static void AppendEnvironmentVariables(this LaunchConfiguration config, ConfigurationAggregator configurationAggregator)
      {
         var environment = configurationAggregator.QueryEnvironmentVariables();
         foreach(var ev in environment)
         {
            config.Environment.Add(new EnvironmentEntry { Name = ev.Key, Value = ev.Value });
         }
      }
   }
}
