// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebRazor.Pages.PrivacyModel.logger")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebRazor.Pages.IndexModel.logger")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "Just for demo purposes", Scope = "member", Target = "~F:WebRazor.Pages.ErrorModel.logger")]
