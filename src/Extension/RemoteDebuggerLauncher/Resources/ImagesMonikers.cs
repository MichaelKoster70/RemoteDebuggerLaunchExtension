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
   public static class ImagesMonikers
   {
      private static readonly Guid manifestGuid = new Guid("C7D7D241-8FD3-4230-8F49-314652792839");
      private const int LaunchProfileIcon = 0;

      public static ImageMoniker LaunchProfileIconImageMoniker
      {
         get
         {
            return new ImageMoniker { Guid = manifestGuid, Id = LaunchProfileIcon };
         }
      }
   }
}
