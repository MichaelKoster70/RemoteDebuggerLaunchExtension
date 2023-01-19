// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Utility class holding the <see cref="ImageMoniker"/> for the images provided by the package.
   /// </summary>
   public static class ImagesMonikers
   {
      private static readonly Guid manifestGuid = new Guid("C7D7D241-8FD3-4230-8F49-314652792839");
      private const int LaunchProfileIcon = 0;
      private const int DeployOutputIcon = 1;
      private const int CleanOutputIcon = 2;
      private const int InstallDebuggerIcon = 3;
      private const int InstallDotnetIcon = 4;
      private const int SetupSshIcon = 5;

      public static ImageMoniker LaunchProfileIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = LaunchProfileIcon };
      public static ImageMoniker DeployOutputIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = DeployOutputIcon };
      public static ImageMoniker CleanOutputIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = CleanOutputIcon };
      public static ImageMoniker InstallDebuggerIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = InstallDebuggerIcon };
      public static ImageMoniker InstallDotnetIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = InstallDotnetIcon };
      public static ImageMoniker SetupSshIconImageMoniker => new ImageMoniker { Guid = manifestGuid, Id = SetupSshIcon };
   }
}
