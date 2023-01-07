// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command handler
   /// </summary>
   internal sealed class OpenToolWindowCommand
   {
      /// <summary>
      /// Command ID.
      /// </summary>
      public const int CommandId = 4129;

      /// <summary>
      /// Command menu group (command set GUID).
      /// </summary>
      public static readonly Guid CommandSet = new Guid("67dde3fd-abea-469b-939f-02a3178c91e7");

      /// <summary>
      /// VS Package that provides this command, not null.
      /// </summary>
      private readonly AsyncPackage package;

      /// <summary>
      /// Initializes a new instance of the <see cref="OpenToolWindowCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private OpenToolWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
      {
         this.package = package ?? throw new ArgumentNullException(nameof(package));
         commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

         var menuCommandID = new CommandID(CommandSet, CommandId);
         var menuItem = new MenuCommand(Execute, menuCommandID);
         commandService.AddCommand(menuItem);
      }

      /// <summary>
      /// Gets the instance of the command.
      /// </summary>
      public static OpenToolWindowCommand Instance
      {
         get;
         private set;
      }

      /// <summary>
      /// Gets the service provider from the owner package.
      /// </summary>
      private IAsyncServiceProvider ServiceProvider => package;

      /// <summary>
      /// Initializes the singleton instance of the command.
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      public static async Task InitializeAsync(AsyncPackage package)
      {
         // Switch to the main thread - the call to AddCommand in ToolWindowCommand's constructor requires the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new OpenToolWindowCommand(package, commandService);
      }

      /// <summary>
      /// Shows the tool window when the menu item is clicked.
      /// </summary>
      /// <param name="sender">The event sender.</param>
      /// <param name="e">The event args.</param>
      private void Execute(object sender, EventArgs e)
      {
         _ = package.JoinableTaskFactory.RunAsync(async delegate
         {
            ToolWindowPane window = await package.ShowToolWindowAsync(typeof(ToolWindow), 0, true, package.DisposalToken);
            if ((null == window) || (null == window.Frame))
            {
               throw new NotSupportedException("Cannot create tool window");
            }
         });
      }
   }
}
