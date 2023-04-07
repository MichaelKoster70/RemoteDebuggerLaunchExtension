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
   /// Utility class providing <see langword="string"/> extension methods.
   /// </summary>
   internal static class StringExtensions
   {
      public static string Unquote(this string s) => !s.StartsWith("\"", StringComparison.Ordinal) || !s.EndsWith("\"", StringComparison.Ordinal) ? s : s.Substring(1, s.Length - 2);
   }
}
