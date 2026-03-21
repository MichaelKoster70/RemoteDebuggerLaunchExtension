// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Shell;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Implements the Local Options page shown in the VS options tree under "Linux Remote Debugger"
   /// </summary>
   [ComVisible(true)]
   [Guid(PackageConstants.Options.PageGuidLocal)]
   internal class LocalOptionsPage : DialogPage
   {
      [Category(PackageConstants.Options.PageCategoryPublish)]
      [DisplayName("Publish on deploy")]
      [Description("Publishes application before deployment. This is useful for application types that require additional files to be deployed like ASP.NET/Blazer.")]
      public bool PublishOnDeploy { get; set; } = false;

      [Category(PackageConstants.Options.PageCategoryPublish)]
      [DisplayName("Publish mode")]
      [Description("The type of application the publish step should produce, either self contained (includes the runtime) or framework dependent (requires .NET to be installed on the device.")]
      public PublishMode PublishMode { get; set; } = PublishMode.FrameworkDependent;

      [Category(PackageConstants.Options.PageCategoryDiagnostics)]
      [DisplayName("Log level")]
      [Description("The minimum logging level for diagnostics. Set to 'None' to disable logging. (requires restart)")]
      [DefaultValue(LogLevel.None)]
      public LogLevel LogLevel { get; set; } = LogLevel.None;
   }
}
