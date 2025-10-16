// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Just for demo purposes", Scope = "member", Target = "~M:WebBlazorWasmApp.Server.Controllers.WeatherForecastController.Get~System.Collections.Generic.IEnumerable{WebBlazorWasmApp.Shared.WeatherForecast}")]
[assembly: SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebBlazorWasmApp.Server.Controllers.WeatherForecastController.logger")]
[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Just for demo purposes", Scope = "member", Target = "~M:WebBlazorWasmApp.Server.Controllers.WeatherForecastController.Get~System.Collections.Generic.IEnumerable{WebBlazorWasmApp.Shared.WeatherForecast}")]
[assembly: SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebBlazorWasmApp.Server.Controllers.WeatherForecastController.logger")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebBlazorWasmApp.Server.Pages.ErrorModel.logger")]
