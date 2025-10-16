// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Utility class for parsing additional deployment configuration strings.
   /// </summary>
   internal class AdditionalDeploymentParser
   {
      private readonly string localProjectPath;
      private readonly string remoteAppFolderPath;

      /// <summary>
      /// Initializes a new instance of the <see cref="AdditionalDeploymentParser"/> class.
      /// </summary>
      /// <param name="localProjectPath">The local project path.</param>
      /// <param name="remoteAppFolderPath">The remote application folder path.</param>
      /// <exception cref="System.ArgumentNullException">localProjectPath</exception>
      public AdditionalDeploymentParser(string localProjectPath, string remoteAppFolderPath)
      {
         this.localProjectPath = localProjectPath ?? throw new ArgumentNullException(nameof(localProjectPath));
         this.remoteAppFolderPath = remoteAppFolderPath ?? throw new ArgumentNullException(nameof(remoteAppFolderPath));
      }

      /// <summary>
      /// Parses a configuration string containing multiple file or folder mappings.
      /// </summary>
      /// <param name="configurationString">The configuration string in format 'source1|target1;source2|target2'.</param>
      /// <returns>A list of <see cref="AdditionalDeploymentEntry"/> objects.</returns>
      /// <exception cref="ArgumentException">Thrown when the configuration string format is invalid.</exception>
      public IList<AdditionalDeploymentEntry> Parse(string configurationString)
      {
         var result = new List<AdditionalDeploymentEntry>();

         if (!string.IsNullOrEmpty(configurationString))
         {

            // Split by semicolon to get individual entries
            var entries = configurationString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
               var trimmedEntry = entry.Trim();
               if (string.IsNullOrEmpty(trimmedEntry))
               {
                  continue;
               }

               // Split by pipe to get source and target
               var parts = trimmedEntry.Split(new char[] { '|' }, StringSplitOptions.None);
               if (parts.Length != 2)
               {
                  throw new ArgumentException($"Invalid additional deployment entry format: '{trimmedEntry}'. Expected format: 'source|target'.", nameof(configurationString));
               }

               var sourcePath = parts[0].Trim();
               var targetPath = parts[1].Trim();

               if (string.IsNullOrEmpty(sourcePath))
               {
                  throw new ArgumentException($"Source path cannot be empty in entry: '{trimmedEntry}'.", nameof(configurationString));
               }

               if (string.IsNullOrEmpty(targetPath))
               {
                  throw new ArgumentException($"Target path cannot be empty in entry: '{trimmedEntry}'.", nameof(configurationString));
               }

               // If target path is relative, make it relative to the project directory
               if (!Path.IsPathRooted(sourcePath))
               {
                  sourcePath = Path.GetFullPath(Path.Combine(localProjectPath, sourcePath));
               }

               // If target path is relative, make it relative to the remote application folder
               if (!UnixPath.IsPathRooted(targetPath))
               {
                  targetPath = UnixPath.Combine(remoteAppFolderPath, targetPath);
               }



               result.Add(new AdditionalDeploymentEntry(sourcePath, targetPath));
            }
         }

         return result;
      }
   }
}