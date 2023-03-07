// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.IO;

namespace RemoteDebuggerLauncher.WebTools
{
   /// <summary>
   /// Browser target detector base class.
   /// </summary>
   internal abstract class BrowserTargetDetector
   {
      protected BrowserTargetDetector(BrowserKind kind = BrowserKind.Default) => Kind = kind;

      public BrowserKind Kind { get; }

      public virtual string NewWindowCommandLineArgument => null;

      public bool MatchesLauncher(string executable)
      {
         if (string.IsNullOrEmpty(LauncherExecutable) || string.IsNullOrEmpty(executable))
         {
            return false;
         }

         try
         {
            executable = Path.GetFileNameWithoutExtension(executable);
         }
         catch (ArgumentException)
         {
            executable = null;
         }

         return string.Equals(LauncherExecutable, executable, StringComparison.OrdinalIgnoreCase);
      }

      protected abstract string LauncherExecutable { get; }
   }

   /// <summary>
   /// Browser target detector for Microsoft Edge (chromium).
   /// </summary>
   internal class MsEdgeBrowserTargetDetector : BrowserTargetDetector
   {
      public MsEdgeBrowserTargetDetector() : base(BrowserKind.MSEdge)
      {
         //EMPTY_BODY
      }

      public override string NewWindowCommandLineArgument => " -new-window ";

      protected override string LauncherExecutable => "msedge";
   }

   /// <summary>
   /// Browser target detector for Google Chrome
   /// </summary>
   internal class ChromeBrowserTargetDetector : BrowserTargetDetector 
   {
      public ChromeBrowserTargetDetector() : base(BrowserKind.Chrome)
      {
         //EMPTY_BODY
      }

      public override string NewWindowCommandLineArgument => " -new-window ";

      protected override string LauncherExecutable => "chrome";
   }

   internal class DefaultBrowserTargetDetector : BrowserTargetDetector
   {
      private readonly string processName;

      public DefaultBrowserTargetDetector(string executable)
      {
         ThrowIf.ArgumentNullOrEmpty(executable, nameof(executable));

         processName = Path.GetFileNameWithoutExtension(executable) ?? string.Empty;
      }

      protected override string LauncherExecutable => processName;
   }
}
