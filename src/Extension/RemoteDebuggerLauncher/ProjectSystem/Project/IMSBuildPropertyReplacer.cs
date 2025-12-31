// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Interface for a service that can replace MSBuild properties $(...) in strings.
   /// </summary>
   internal interface IMSBuildPropertyReplacer
   {
      /// <summary>
      /// Replaces MSBuild properties in the given input string.
      /// </summary>
      /// <param name="input">The input string possibly containing MSBuild properties.</param>
      /// <returns>The string with replaced properties.</returns>
      /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <c>null</c>.</exception>
      Task<string> ReplacePropertiesAsync(string input); 
   }
}
