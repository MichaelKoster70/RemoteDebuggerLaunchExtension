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
   internal static class AdapterLaunchConfiguration
   {
      internal class LaunchConfiguration
      {
         [JsonProperty("version")]
         public string Version => "0.2.0";

         [JsonProperty("name")]
         public string Name => ".NET Core Launch";

         [JsonProperty("request")]
         public string Request => "launch";

         [JsonProperty("type")]
         public string Type => "coreclr";

         [JsonProperty("$adapter")]
         public string Adapter { get; set; }

         [JsonProperty("$adapterArgs")]
         public string AdapterArgs { get; set; }

         /// <summary>The program to debug</summary>
         /// <remarks>Must be the path to dotnet(.exe)</remarks>
         [JsonProperty("program")]
         public string Program { get; set; }

         /// <summary>Command line arguments, the first being the .NET assembly to launch</summary>
         /// <value>The arguments.</value>
         [JsonProperty("args")]
         public List<string> Args { get; } = new List<string>();

         /// <summary>Additional settings</summary>
         [JsonExtensionData]
         public Dictionary<string, JToken> ConfigurationProperties { get; } = new Dictionary<string, JToken>();
      }

      public static async Task<string> CreateAsync(ConfigurationAggregator configurationAggregator, ConfiguredProject configuredProject)
      {
         ThrowIf.ArgumentNull(configurationAggregator, nameof(configurationAggregator));
         ThrowIf.ArgumentNull(configuredProject, nameof(configuredProject));

         var provider = configurationAggregator.QueryAdapterProvider();
         var hostName = configurationAggregator.QueryHostName();
         var userName = configurationAggregator.QueryUserName();
         var privateKey = configurationAggregator.QueryPrivateKeyFilePath();
         var program = UnixPath.Combine(configurationAggregator.QueryDotNetInstallFolderPath(), PackageConstants.BinaryNameDotnet);
         var vsdbgPath = UnixPath.Combine(configurationAggregator.QueryDebuggerInstallFolderPath(), PackageConstants.BinaryNameDebugger);

         var appFolderPath = configurationAggregator.QueryAppFolderPath();
         var assemblyFileName = await configuredProject.GetAssemblyFileNameAsync();

         var assemblyPath = UnixPath.Combine(appFolderPath, assemblyFileName);

         string adapter = string.Empty;
         string adapterArgs = string.Empty;
         switch(provider)
         {
            case AdapterProviderKind.WindowsSSH:
               adapter = PackageConstants.AdapterNameWindowsSSH;
               adapterArgs += string.IsNullOrEmpty(privateKey) ? $"-i {privateKey}" : string.Empty;
               adapterArgs += $"{userName}@{hostName} {vsdbgPath} --interpreter=vscode";
               break;

            case AdapterProviderKind.PuTTY:
               adapter = PackageConstants.AdapterNameWindowsSSH;
               break;
         }

         var config = new LaunchConfiguration()
         {
            Adapter = adapter,
            AdapterArgs = adapterArgs,
            Program = program,
            Args =
            {
               assemblyPath
            }
         };

         return JsonConvert.SerializeObject(config);
      }
   }
}
