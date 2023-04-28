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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using RemoteDebuggerLauncher.SecureShell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command handler for the Setup HTTPS command.
   /// </summary>
   internal sealed class SetupHttpsCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0105;

      /// <summary>VS Package that provides this command, not null.</summary>
      public static readonly Guid CommandSet = new Guid("67dde3fd-abea-469b-939f-02a3178c91e7");

      /// <summary>
      /// VS Package that provides this command, not null.
      /// </summary>
      private readonly AsyncPackage package;

      /// <summary>
      /// Initializes a new instance of the <see cref="SetupHttpsCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private SetupHttpsCommand(AsyncPackage package, OleMenuCommandService commandService)
      {
         this.package = package ?? throw new ArgumentNullException(nameof(package));
         commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

         var menuCommandID = new CommandID(PackageConstants.Commands.CommandSet, CommandId);
         var menuItem = new MenuCommand(Execute, menuCommandID);
         commandService.AddCommand(menuItem);
      }

      /// <summary>
      /// Gets the instance of the command.
      /// </summary>
      public static SetupHttpsCommand Instance
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
         // Switch to the main thread - the call to AddCommand in SetupHttps's constructor requires
         // the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new SetupHttpsCommand(package, commandService);
      }

      /// <summary>
      /// This function is the callback used to execute the command when the menu item is clicked.
      /// </summary>
      /// <param name="sender">Event sender.</param>
      /// <param name="e">Event args.</param>
      /// <remarks>
      /// See the constructor to see how the menu item is associated with this function using OleMenuCommandService service and MenuCommand class.
      /// </remarks>
      private void Execute(object sender, EventArgs e)
      {
         ThreadHelper.ThrowIfNotOnUIThread();

         // bring up config dialog
         var viewModel = new SetupHttpsViewModel(ThreadHelper.JoinableTaskFactory);
         var dialog = new SetupHttpsDialogWindow()
         {
            DataContext = viewModel
         };

         var result = dialog.ShowDialog();

         // process 
         if (result.HasValue && result.Value)
         {

         }
      }
   }
}
