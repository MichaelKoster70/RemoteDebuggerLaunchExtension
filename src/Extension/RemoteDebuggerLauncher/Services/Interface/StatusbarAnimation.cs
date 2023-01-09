// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Defines the supported animations.
   /// </summary>
   internal enum StatusbarAnimation : short
   {
      /// <summary>Standard animation icon.</summary>
      General = 0,

      /// <summary>The deploy animation icon.</summary>
      Deploy = 3,

      /// <summary>The build animation icon.</summary>
      Build = 5
   }
}
