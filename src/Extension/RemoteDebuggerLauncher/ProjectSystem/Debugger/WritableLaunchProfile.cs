// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace RemoteDebuggerLauncher.ProjectSystem.Debugger
{
   /// <summary>
   /// Provides a lightweight, writable implementation of a launch profile.
   /// Implements the <see cref="IWritableLaunchProfile" />
   /// </summary>
   /// <seealso cref="IWritableLaunchProfile" />
   /// <remarks>
   /// This is essentially a clone <c>Microsoft.VisualStudio.ProjectSystem.Debug.WritableLaunchProfile</c> class 
   /// and only needed because that class is internal
   /// </remarks>
   internal class WritableLaunchProfile : IWritableLaunchProfile
   {
      public string Name { get; set; }

      public string CommandName { get; set; }

      public string ExecutablePath { get; set; }

      public string CommandLineArgs { get; set; }

      public string WorkingDirectory { get; set; }

      public bool LaunchBrowser { get; set; }

      public string LaunchUrl { get; set; }

      public Dictionary<string, string> EnvironmentVariables { get; }

      public Dictionary<string, object> OtherSettings { get; }

      public WritableLaunchProfile(ILaunchProfile profile)
      {
         Name = profile.Name;
         ExecutablePath = profile.ExecutablePath;
         CommandName = profile.CommandName;
         CommandLineArgs = profile.CommandLineArgs;
         WorkingDirectory = profile.WorkingDirectory;
         LaunchBrowser = profile.LaunchBrowser;
         LaunchUrl = profile.LaunchUrl;
         EnvironmentVariables = ToDictionary(profile.EnvironmentVariables, StringComparer.OrdinalIgnoreCase);
         OtherSettings = ToDictionary(profile.OtherSettings, StringComparer.Ordinal);
      }

      private static Dictionary<string, T> ToDictionary<T>(ImmutableDictionary<string, T> source, IEqualityComparer<string> comparer)
      {
         var result = new Dictionary<string, T>(comparer);

         foreach (var pair in source)
         {
            result[pair.Key] = pair.Value;
         }

         return result;
      }

      public ILaunchProfile ToLaunchProfile()
      {
         return new LaunchProfile(
            name: Name, 
            commandName: CommandName, 
            executablePath: ExecutablePath, 
            commandLineArgs: CommandLineArgs, 
            workingDirectory: WorkingDirectory, 
            launchBrowser: LaunchBrowser, 
            launchUrl: LaunchUrl, 
            environmentVariables: ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, EnvironmentVariables), 
            otherSettings: ImmutableDictionary.CreateRange(StringComparer.Ordinal, OtherSettings));
      }
   }
}
