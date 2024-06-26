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
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "required in order to safely ignore errors", Scope = "member", Target = "~P:RemoteDebuggerLauncher.WebTools.ConfiguredWebProject.WebRoot")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "required in order to safely ignore errors", Scope = "member", Target = "~M:RemoteDebuggerLauncher.WebTools.StaticWebAssetsCollectorService.LoadStaticAssetsAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Dotnet.GetInstallDotnetShUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Dotnet.GetInstallDotnetPs1Url")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Debugger.GetVsDbgPs1Url")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "By design, is a well-known URL", Scope = "member", Target = "~F:RemoteDebuggerLauncher.PackageConstants.Debugger.GetVsDbgShUrl")]
[assembly: SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Dispose used to control the lifetime of the instance only.", Scope = "type", Target = "~T:RemoteDebuggerLauncher.WaitDialogService")]
[assembly: SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "By design, is a well-known term", Scope = "type", Target = "~T:RemoteDebuggerLauncher.PackageConstants.CPS")]
[assembly: SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "By design", Scope = "member", Target = "~M:RemoteDebuggerLauncher.DotnetPublishService.SupportsFrameworkDependantAsync~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to not crash the process on an expecption", Scope = "member", Target = "~M:RemoteDebuggerLauncher.DotnetPublishService.StartAsync~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "By design to ignore all failures", Scope = "member", Target = "~M:RemoteDebuggerLauncher.WebTools.CertificateService.DisposeCertificates(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)")]
