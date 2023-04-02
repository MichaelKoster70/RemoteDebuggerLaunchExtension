// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using RemoteDebuggerLauncher.WebTools;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// The browser debug launch settings provider.
   /// Implements <see cref="IBrowserDebugLaunchSettingsProvider"/>
   /// </summary>
   [Export(typeof(IBrowserDebugLaunchSettingsProvider))]
   internal class BrowserDebugLaunchSettingsProvider : IBrowserDebugLaunchSettingsProvider
   {
      /// <summary>
      /// The JSON settings for the chrome based debug engines.
      /// </summary>
      private sealed class ChromeDebugArgJsonSettings
      {
         [JsonProperty("browserExe")]
         public string BrowserExe { get; set; }

         [JsonProperty("browserLaunchOptions")]
         public string BrowserLaunchOptions { get; set; }

         [JsonProperty("continueOnDisconnect")]
         public bool ContinueOnDisconnect { get; set; }

         [JsonProperty("userDataDir")]
         public string UserDataDir { get; set; }

         [JsonProperty("url")]
         public string Url { get; set; }

         [JsonProperty("projectGuid")]
         public string ProjectGuid { get; set; }

         [JsonProperty("inspectUri")]
         public string InspectUri { get; set; }

         [JsonProperty("type")]
         public string Type { get; set; }

         [JsonProperty("webRoot")]
         public string WebRoot { get; set; }

         [JsonProperty("pathMapping")]
         public IReadOnlyDictionary<string, string> PathMapping { get; set; }

         [JsonProperty("__webAssemblyDebugServerPath")]
         public string WebAssemblyDebugServerPath { get; set; }

         [JsonProperty("__startWebAssemblyDebugServer")]
         public bool StartWebAssemblyDebugServer { get; set; }
      }

      public static readonly Guid JavaScriptChromeDevToolsProtocolDebugEngine = new Guid("{394120B6-2FF9-4D0D-8953-913EF5CD0BCD}");

      private readonly IVsFacadeFactory facadeFactory;
      private readonly ConfiguredProject configuredProject;
      private readonly IWebBrowserSelectionService browserSelectionService;
      private readonly IConfiguredWebProject webProjectInformation;
      private readonly IVsRegistryOptions registryOptions;

      [ImportingConstructor]
      public BrowserDebugLaunchSettingsProvider(IVsFacadeFactory facadeFactory, ConfiguredProject configuredProject, IWebBrowserSelectionService browserSelectionService, IConfiguredWebProject webProjectInformation, IVsRegistryOptions registryOptions)
      {
         this.facadeFactory = facadeFactory;
         this.configuredProject = configuredProject;
         this.browserSelectionService = browserSelectionService;
         this.webProjectInformation = webProjectInformation;
         this.registryOptions = registryOptions;
      }

      /// <inheritdoc />
      public async Task<IList<DebugLaunchSettings>> GetLaunchSettingsAsync(Uri browseUri, DebugLaunchOptions launchOptions, ConfigurationAggregator configuration)
      {
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

         var webRoot = webProjectInformation.WebRoot;
         var projectFacade = facadeFactory.GetVsProject(configuredProject.UnconfiguredProject.Services.HostObject as IVsHierarchy);
         var pathMapping = await webProjectInformation.StaticWebAssets.GetPathMappingsAsync();

         List<DebugLaunchSettings> launchTargets = new List<DebugLaunchSettings>();
         if (launchOptions.IsDebugging())
         {
            var defaultBrowser = browserSelectionService.GetDefaultBrowserForDebug();
            launchTargets.Add(ComposeLaunchSettings(defaultBrowser, browseUri, webRoot, projectFacade, launchOptions, pathMapping));
         }
         else
         {
            foreach (var defaultBrowser in browserSelectionService.GetDefaultBrowsers())
            {
               launchTargets.Add(ComposeLaunchSettings(defaultBrowser, browseUri, webRoot, projectFacade, launchOptions,  null));
            }
         }
         return launchTargets;
      }

      private DebugLaunchSettings ComposeLaunchSettings(WebBrowserInfo browserInfo, Uri browseUri, string webRoot, IVsProjectFacade projectFacade, DebugLaunchOptions launchOptions, IReadOnlyDictionary<string, string> pathMapping)
      {
         DebugLaunchSettings browserLaunchSettings = new DebugLaunchSettings(launchOptions)
         {
            Project = projectFacade.ProjectHierarchy,
            LaunchOperation = DebugLaunchOperation.CreateProcess
         };

         bool attachDebugger = false;
         if (launchOptions.IsDebugging() && BrowserSupportsDebugging(browserInfo.Kind))
         {
            attachDebugger = facadeFactory.GetVsDebugger().IsJavaScriptDebuggingOnLaunchEnabled();
            if (!attachDebugger)
            {
               attachDebugger = facadeFactory.GetVsDebugger().NotifyBeforeLaunchWithoutJavaScriptDebugger();
            }
         }

         string browserArgs = (launchOptions.IsDebugging() && !attachDebugger && registryOptions.WebToolsEnableDebugTargetsObserver) ? browserInfo.NewWindowCommandLineArguments : browserInfo.Arguments;
         browserLaunchSettings.Options = "*";
         if (attachDebugger && BrowserIsChromiumBased(browserInfo.Kind))
         {
            string encodedUrl = browseUri.AbsoluteUri.ToString();
            browserLaunchSettings.Executable = encodedUrl;
            browserLaunchSettings.LaunchDebugEngineGuid = JavaScriptChromeDevToolsProtocolDebugEngine;
            browserLaunchSettings.Options = GetChromiumOptions(encodedUrl, browserInfo.Path, browserArgs, webRoot, projectFacade.ProjectGuidString, browserInfo.Kind, pathMapping);
            browserLaunchSettings.LaunchOptions = launchOptions | DebugLaunchOptions.StopDebuggingOnEnd;
         }
         else
         {
            browserLaunchSettings.Executable = browserInfo.Path;
            browserLaunchSettings.Arguments = $"{browserArgs} {browseUri}";
            browserLaunchSettings.LaunchOptions = launchOptions | DebugLaunchOptions.NoDebug;
         }
         return browserLaunchSettings;
      }

      private string GetChromiumOptions(string encodedUrl, string browserPath,string browserArgs, string webRoot, string projectGuid, BrowserKind browserTarget, IReadOnlyDictionary<string, string> pathMapping)
      {
         var debugArgJsonSettings = new ChromeDebugArgJsonSettings()
         {
            BrowserExe = browserPath,
            BrowserLaunchOptions = browserArgs ?? string.Empty,
            ContinueOnDisconnect = true,
            Url = encodedUrl,
            ProjectGuid = projectGuid,
            Type = browserTarget == BrowserKind.Chrome ? "chrome" : "edge",
            WebRoot = webRoot,
            PathMapping = pathMapping,
            InspectUri = null, // we do not yet support WASM debugging
         };

         if (browserArgs == null || browserArgs.IndexOf("--user-data-dir", StringComparison.OrdinalIgnoreCase) == -1)
         {
            // Get the path where VS stores local data
            var localToolPath = Path.Combine(facadeFactory.GetVsShell().GetLocalAppDataPath(), "WebTools");
            var userDataDir = Path.Combine(localToolPath, $"{browserPath.ToUpperInvariant().GetHashCode():X} _ {projectGuid.ToUpperInvariant().GetHashCode():X}");
            if (DirectoryHelper.EnsureExists(userDataDir))
            {
               debugArgJsonSettings.UserDataDir = userDataDir;
            }
         }

         return JsonConvert.SerializeObject(debugArgJsonSettings, Formatting.None, new JsonSerializerSettings()
         {
            NullValueHandling = NullValueHandling.Ignore
         });
      }

      private static bool BrowserSupportsDebugging(BrowserKind browserKind) => browserKind == BrowserKind.Chrome || browserKind == BrowserKind.MSEdge;

      private static bool BrowserIsChromiumBased(BrowserKind browserKind) => browserKind == BrowserKind.Chrome || browserKind == BrowserKind.MSEdge;
   }
}
