// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Helper class providing validation services.
   /// </summary>
   internal static class ThrowIf
   {
      /// <summary>
      /// Throws an exception if the supplied component is null.
      /// </summary>
      /// <typeparam name="T">The typeof component</typeparam>
      /// <param name="component">The component instance to validate.</param>
      /// <exception cref="System.InvalidOperationException"></exception>
      public static void NotPresent<T>(T component)
      {
         if (component == null)
         {
            var componentType = typeof(T);
            throw new InvalidOperationException($"{componentType.FullName} is not present");
         }
      }
   }
}
