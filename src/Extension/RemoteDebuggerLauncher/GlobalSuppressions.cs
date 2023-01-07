// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "Signature given by VS Package", Scope = "member", Target = "~M:RemoteDebuggerLauncher.RemoteDebuggerLauncherPackage.CreateServiceAsync(Microsoft.VisualStudio.Shell.IAsyncServiceContainer,System.Threading.CancellationToken,System.Type)~System.Threading.Tasks.Task{System.Object}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be an instance property", Scope = "member", Target = "~P:RemoteDebuggerLauncher.AdapterLaunchConfiguration.LaunchConfiguration.Type")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be an instance property", Scope = "member", Target = "~P:RemoteDebuggerLauncher.AdapterLaunchConfiguration.LaunchConfiguration.Console")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be an instance property", Scope = "member", Target = "~P:RemoteDebuggerLauncher.AdapterLaunchConfiguration.LaunchConfiguration.Version")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needs to be an instance property", Scope = "member", Target = "~P:RemoteDebuggerLauncher.AdapterLaunchConfiguration.LaunchConfiguration.Request")]
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>", Scope = "member", Target = "~P:RemoteDebuggerLauncher.OpenToolWindowCommand.ServiceProvider")]
