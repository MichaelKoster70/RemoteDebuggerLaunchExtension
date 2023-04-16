// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using RemoteDebuggerLauncher.SecureShell;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Command handler for the Setup SSH command.
   /// </summary>
   internal sealed class SetupSshCommand
   {
      /// <summary>Command ID.</summary>
      public const int CommandId = 0x0104;

      /// <summary>VS Package that provides this command, not null.</summary>
      private readonly AsyncPackage package;

      /// <summary>
      /// Initializes a new instance of the <see cref="SetupSshCommand"/> class.
      /// Adds our command handlers for menu (commands must exist in the command table file)
      /// </summary>
      /// <param name="package">Owner package, not null.</param>
      /// <param name="commandService">Command service to add command to, not null.</param>
      private SetupSshCommand(AsyncPackage package, OleMenuCommandService commandService)
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
      public static SetupSshCommand Instance
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
         // Switch to the main thread - the call to AddCommand in SetupSshCommand's constructor requires
         // the UI thread.
         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

         OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         Instance = new SetupSshCommand(package, commandService);
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
         var viewModel = new SetupSshViewModel(ThreadHelper.JoinableTaskFactory, new SecureShellKeyPairCreatorService());
         var dialog = new SetupSshDialogWindow()
         {
            DataContext = viewModel
         };

         var result = dialog.ShowDialog();

         // process 
         if (result.HasValue && result.Value)
         {
            var setting = new SecureShellKeySetupSettings(viewModel);
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            package.JoinableTaskFactory.Run(async () =>
            {
               var keySetup = await ServiceProvider.GetSecureShellKeySetupServiceAsync();

#pragma warning disable CA1031 // Do not catch general exception types
               try
               {
                  // do the remaining work on the UI thread
                  await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                  await keySetup.RegisterServerFingerprintAsync(setting);
                  await keySetup.AuthorizeKeyAsync(setting);
               }
               catch (Exception exception)
               {
                  ShellUtilities.ShowErrorMessageBox(package, Resources.RemoteCommandSetupSshCommandCaption, exception.Message);
               }
               finally
               {
                  keySetup.Statusbar.Clear();
               }
            });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
         }
      }
   }
}
