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
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
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

         /// <summary>The VS Code console option.</summary>
         /// <remarks>Have no effect in a remote debugger scenario.</remarks>
         [JsonProperty("console")]
         public string Console => "internalConsole";

         /// <summary>Additional settings</summary>
         [JsonExtensionData]
         public Dictionary<string, JToken> ConfigurationProperties { get; } = new Dictionary<string, JToken>();
      }

      public static async Task<string> CreateFrameworkDependantAsync(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));

         var program = UnixPath.Combine(configurationAggregator.QueryDotNetInstallFolderPath(), PackageConstants.Dotnet.BinaryName);
         var appFolderPath = configurationAggregator.QueryAppFolderPath();
         var assemblyFileName = await configuredProject.GetAssemblyFileNameAsync();

         var config = CreateAndSetAdapter(configurationAggregator);
         config.Name = ".NET Core Launch - Framework dependant";
         config.Program = program;
         config.Args.Add($"./{assemblyFileName}");
         config.CurrentWorkingDirectory = appFolderPath;
         config.AppendCommandLineArguments(configurationAggregator);

         return JsonConvert.SerializeObject(config);
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

         return JsonConvert.SerializeObject(config);
      }

      private static LaunchConfiguration CreateAndSetAdapter(ConfigurationAggregator configurationAggregator)
      {
         // Collect the configurable options
         var provider = configurationAggregator.QueryAdapterProvider();
         var hostName = configurationAggregator.QueryHostName();
         var userName = configurationAggregator.QueryUserName();
         var privateKey = configurationAggregator.QueryPrivateKeyFilePath();
         var vsdbgPath = UnixPath.Combine(configurationAggregator.QueryDebuggerInstallFolderPath(), PackageConstants.Debugger.BinaryName);

         // assemble the adapter and adapterArg values
         string adapter = string.Empty;
         string adapterArgs = string.Empty;
         switch (provider)
         {
            case AdapterProviderKind.WindowsSSH:
               adapter = PackageConstants.DebugLaunchSettings.Options.AdapterNameWindowsSSH;
               adapterArgs += string.IsNullOrEmpty(privateKey) ? $"-i {privateKey}" : string.Empty;
               adapterArgs += $"{userName}@{hostName} {vsdbgPath} --interpreter=vscode";
               break;

            case AdapterProviderKind.PuTTY:
               adapter = PackageConstants.DebugLaunchSettings.Options.AdapterNamePuTTY;
               adapterArgs += string.IsNullOrEmpty(privateKey) ? $"-i {privateKey}" : string.Empty;
               adapterArgs += $"{userName}@{hostName} {vsdbgPath} --interpreter=vscode";
               break;
         }

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

         if (!String.IsNullOrEmpty(commandLineArgs))
         {
            var args = commandLineArgs.Split(' ');
            Array.ForEach(args, (arg) => config.Args.Add(arg));
         }
      }
   }
}
