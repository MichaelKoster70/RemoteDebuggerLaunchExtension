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
   /// <remarks>
   /// The parser assumes 
   /// * the configuration string is in the format 'source1|target1;source2|target2',
   /// * target path ending with a / indicates a folder deployment
   /// </remarks>
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
      /// <param name="file">Indicates whether the mapping is for a file or a folder.</param>
      /// <returns>A list of <see cref="AdditionalDeploymentEntry"/> objects.</returns>
      /// <exception cref="ArgumentException">Thrown when the configuration string format is invalid.</exception>
      public IList<AdditionalDeploymentEntry> Parse(string configurationString, bool file)
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
               var (sourcePath, targetPath) = ParseEntry(trimmedEntry);

               // If target path is relative, make it relative to the project directory
               sourcePath = MakeAbsoluteLocalPath(sourcePath);

               // If target path is relative, make it relative to the remote application folder
               targetPath = MakeAbsoluteRemotePath(targetPath);

               if (file)
               {
                  // in case the target path denotes a folder, append the file name
                  if (UnixPath.DenotesFolder(targetPath))
                  {
                     var fileName = Path.GetFileName(sourcePath);
                     targetPath = UnixPath.Combine(targetPath, fileName);
                  }
               }
               else
               {
                  // For directories, ensure trailing slash
                  targetPath = UnixPath.AppendTrailingSlash(targetPath);
               }

               result.Add(new AdditionalDeploymentEntry(sourcePath, targetPath));
            }
         }

         return result;
      }

      private static (string sourcePath, string targetPath) ParseEntry(string entry)
      {
         var parts = entry.Split(new char[] { '|' }, StringSplitOptions.None);
         if (parts.Length != 2)
         {
            throw new ArgumentException($"Invalid additional deployment entry format: '{entry}'. Expected format: 'source|target'.", nameof(entry));
         }

         var sourcePath = parts[0].Trim();
         var targetPath = parts[1].Trim();
         
         if (string.IsNullOrEmpty(sourcePath))
         {
            throw new ArgumentException($"Source path cannot be empty in entry: '{entry}'.", nameof(entry));
         }
         
         if (string.IsNullOrEmpty(targetPath))
         {
            throw new ArgumentException($"Target path cannot be empty in entry: '{entry}'.", nameof(entry));
         }
         return (sourcePath, targetPath);
      }

      private string MakeAbsoluteLocalPath(string path)
      {
         if (Path.IsPathRooted(path))
         {
            return path;
         }
         else
         {
            return Path.GetFullPath(Path.Combine(localProjectPath, path));
         }
      }

      private string MakeAbsoluteRemotePath(string path)
      {
         if (UnixPath.IsPathRooted(path))
         {
            return path;
         }
         else
         {
            return UnixPath.Combine(remoteAppFolderPath, path);
         }
      }
   }
}