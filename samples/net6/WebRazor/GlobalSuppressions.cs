// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "For demo purposes only", Scope = "type", Target = "~T:WebRazor.Pages.IndexModel")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "For demo purposes only", Scope = "type", Target = "~T:WebRazor.Pages.PrivacyModel")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "For demo purposes only", Scope = "type", Target = "~T:WebRazor.Pages.ErrorModel")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "For demo purposes only", Scope = "member", Target = "~F:WebRazor.Pages.ErrorModel.logger")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "For demo purposes only", Scope = "member", Target = "~F:WebRazor.Pages.IndexModel.logger")]
[assembly: SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "For demo purposes only", Scope = "member", Target = "~F:WebRazor.Pages.PrivacyModel.logger")]
