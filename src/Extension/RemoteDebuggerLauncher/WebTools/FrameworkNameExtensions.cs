// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.Versioning;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Holds extension methods for the <see cref="FrameworkName"/> type.
   /// </summary>
   internal static class FrameworkNameExtensions
   {
      public static readonly Version Version60 = new Version(6, 0);
      public const string DotNETCore = ".NETCore";
      public const string DotNETCoreApp = ".NETCoreApp";

      /// <summary>
      /// Determines whether the framework is a.NET (core) app
      /// </summary>
      /// <param name="framework">The framework.</param>
      /// <returns><c>true</c> for a.NET Core app; otherwise, <c>false</c>.</returns>
      public static bool IsNetCoreApp(this FrameworkName framework) => framework.Identifier.Equals(DotNETCoreApp, StringComparison.OrdinalIgnoreCase);

      /// <summary>
      /// Determines whether the framework is a.NET (core)
      /// </summary>
      /// <param name="framework">The framework.</param>
      /// <returns><c>true</c> for a.NET Core; otherwise, <c>false</c>.</returns>
      public static bool IsNetCore(this FrameworkName framework) => framework.Identifier.Equals(DotNETCore, StringComparison.OrdinalIgnoreCase);

      /// <summary>
      /// Determines whether the framework is .NET 6 or newer.
      /// </summary>
      /// <param name="framework">The framework name.</param>
      /// <returns><c>true</c> if .NET 6 or newer; otherwise, <c>false</c>.</returns>
      public static bool IsNET60OrNewer(this FrameworkName framework) => (framework.IsNetCoreApp() || framework.IsNetCore()) && framework.Version >= Version60;
   }
}
