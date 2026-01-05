// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher.Infrastructure
{
   /// <summary>
   /// Represents a single additional deployment entry (file or folder) to be deployed to the remote device.
   /// </summary>
   internal class AdditionalDeploymentEntry
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="AdditionalDeploymentEntry"/> class.
      /// </summary>
      /// <param name="sourcePath">The source path relative to the project directory.</param>
      /// <param name="targetPath">The target path relative to the app folder on the remote device.</param>
      public AdditionalDeploymentEntry(string sourcePath, string targetPath)
      {
         ThrowIf.ArgumentNull(sourcePath, nameof(sourcePath));
         ThrowIf.ArgumentNull(targetPath, nameof(targetPath));

         SourcePath = sourcePath;
         TargetPath = targetPath;
      }

      /// <summary>
      /// Gets the source path relative to the project directory.
      /// </summary>
      public string SourcePath { get; }

      /// <summary>
      /// Gets the target path relative to the app folder on the remote device.
      /// </summary>
      public string TargetPath { get; }
   }
}