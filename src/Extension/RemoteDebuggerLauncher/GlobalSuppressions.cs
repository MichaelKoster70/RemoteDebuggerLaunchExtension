﻿// ----------------------------------------------------------------------------
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
[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "not yet implemented", Scope = "member", Target = "~P:RemoteDebuggerLauncher.OpenToolWindowCommand.ServiceProvider")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "required in order to safely ignore errors", Scope = "member", Target = "~P:RemoteDebuggerLauncher.WebTools.ConfiguredWebProject.WebRoot")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "required in order to safely ignore errors", Scope = "member", Target = "~M:RemoteDebuggerLauncher.WebTools.StaticWebAssetsCollectorService.LoadStaticAssetsAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Dotnet.GetInstallDotnetShUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Dotnet.GetInstallDotnetPs1Url")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Debugger.GetVsDbgPs1Url")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Debugger.GetVsDbgShUrl")]
[assembly: SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Dispose used to control the lifetime of the instance only.", Scope = "type", Target = "~T:RemoteDebuggerLauncher.WaitDialogService")]
