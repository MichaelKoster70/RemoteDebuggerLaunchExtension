// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Not supported in .NET4.8/C# 7.3", Scope = "member", Target = "~M:RemoteDebuggerLauncher.CheckSum.ScanResult.#ctor")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Not supported in .NET4.8/C# 7.3", Scope = "member", Target = "~M:RemoteDebuggerLauncher.CheckSum.DirectoryScanner.GetRelativePath(System.String)~System.String")]
[assembly: SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Not supported in .NET4.8/C# 7.3", Scope = "member", Target = "~M:RemoteDebuggerLauncher.CheckSum.DirectoryScanner.ComputeHashForFile(System.String,RemoteDebuggerLauncher.CheckSum.ScanResult)")]
[assembly: SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Not supported in .NET4.8/C# 7.3", Scope = "member", Target = "~M:RemoteDebuggerLauncher.CheckSum.ScanResult.AddInaccessible(System.String)")]
[assembly: SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Not supported in .NET4.8/C# 7.3", Scope = "member", Target = "~M:RemoteDebuggerLauncher.CheckSum.ScanResult.AddHash(System.String,System.String)")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Unused in .NET8", Scope = "type", Target = "~T:RemoteDebuggerLauncher.CheckSum.DirectoryScanner")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Unused in .NET8", Scope = "type", Target = "~T:RemoteDebuggerLauncher.CheckSum.ScanResult")]
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Justification = "Intentionally", Scope = "namespace", Target = "~N:RemoteDebuggerLauncher.CheckSum")]
