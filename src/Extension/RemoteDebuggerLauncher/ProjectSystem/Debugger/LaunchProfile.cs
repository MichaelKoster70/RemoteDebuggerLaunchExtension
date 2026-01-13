// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Implements a lightweight, immutable launch profile.
   /// </summary>
   /// <seealso cref="ILaunchProfile" />
   /// <seealso cref="IPersistOption" />
   /// <remarks>
   /// This is essentially a clone <c>Microsoft.VisualStudio.ProjectSystem.Debug.Launchprofile</c> class 
   /// and only needed because that class is internal
   /// </remarks>
   internal class LaunchProfile : ILaunchProfile, IPersistOption
   {
      public string Name { get; }

      public string CommandName { get; }

      public string ExecutablePath { get; }

      public string CommandLineArgs { get; }

      public string WorkingDirectory { get; }

      public bool LaunchBrowser { get; }

      public string LaunchUrl { get; }

      public bool DoNotPersist { get; }
      
      public ImmutableDictionary<string, string> EnvironmentVariables { get; }
      
      public ImmutableDictionary<string, object> OtherSettings { get; }

      public LaunchProfile(string name, string commandName, string executablePath = null, string commandLineArgs = null, string workingDirectory = null, bool launchBrowser = false, string launchUrl = null, bool doNotPersist = true, ImmutableDictionary<string, string> environmentVariables = default, ImmutableDictionary<string, object> otherSettings = default)
      {
         Name = name;
         CommandName = commandName;
         ExecutablePath = executablePath;
         CommandLineArgs = commandLineArgs;
         WorkingDirectory = workingDirectory;
         LaunchBrowser = launchBrowser;
         LaunchUrl = launchUrl;
         DoNotPersist = doNotPersist;
         EnvironmentVariables = environmentVariables is null ? ImmutableDictionary<string, string>.Empty : environmentVariables;
         OtherSettings = otherSettings is null ? ImmutableDictionary<string, object>.Empty : otherSettings;
      }
   }
}
