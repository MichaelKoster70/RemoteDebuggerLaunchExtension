// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using RemoteDebuggerLauncher.RemoteOperations;
using RemoteDebuggerLauncher.Shared;

namespace RemoteDebuggerLauncher
{
   public class SetupSshViewModel : ViewModelBase, INotifyDataErrorInfo
   {
      private readonly ISecureShellKeyPairCreatorService keyService;
      private readonly Dictionary<string, IList<object>> validationErrors = new Dictionary<string, IList<object>>();
      private readonly Dictionary<string, IList<ValidationRule>> validationRules = new Dictionary<string, IList<ValidationRule>>();

      private string hostName = string.Empty;
      private int hostPort = 22;
      private string username = string.Empty;
      private string password = string.Empty;
      private string publicKeyFile = string.Empty;
      private string privateKeyFile = string.Empty;

      public SetupSshViewModel(JoinableTaskFactory joinableTaskFactory, ISecureShellKeyPairCreatorService keyService)
      {
         this.keyService = keyService;

         // wire-up commands
         BrowsePublicKeyFileCommand = new DelegateCommand(HandleBrowsePublicKeyFileCommandCommand, null, joinableTaskFactory);
         BrowsePrivateKeyFileCommand = new DelegateCommand(HandleBrowsePrivateKeyFileCommandCommand, null, joinableTaskFactory);
         CreateKeyFileCommand = new DelegateCommand(HandleCreateKeyFileCommandCommand, null, joinableTaskFactory);
         OkCommand = new DelegateCommand<DialogWindow>(HandleOkCommand, canExecute => Validate(), joinableTaskFactory);
         CancelCommand = new DelegateCommand<DialogWindow>(HandleCancelCommand, null, joinableTaskFactory);

         // initialize validation
         validationRules.Add(nameof(HostName), new List<ValidationRule>() { new HostNameValidationRule() });
         validationRules.Add(nameof(HostPort), new List<ValidationRule>() { new HostPortValidationRule() });
         validationRules.Add(nameof(Username), new List<ValidationRule>() { new RegexValidationRule(Constants.RegexExpressions.LinuxUsername, Resources.RegexValidationRuleNoValidUsername) });
         validationRules.Add(nameof(PublicKeyFile), new List<ValidationRule>() { new FileExistsValidationRule(Resources.FileExistsValidationRuleNotPresent) });
         validationRules.Add(nameof(PrivateKeyFile), new List<ValidationRule>() { new FileExistsValidationRule(Resources.FileExistsValidationRuleNotPresent) });
      }

      public string HostName
      {
         get => hostName;
         set
         {
            if (ValidateAndSetProperty(ref hostName, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public int HostPort
      {
         get => hostPort;
         set
         {
            if (ValidateAndSetProperty(ref hostPort, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public string Username
      {
         get => username;
         set
         {
            if (ValidateAndSetProperty(ref username, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public string Password => password;

      public string PublicKeyFile
      {
         get => publicKeyFile;
         set
         {
            if (ValidateAndSetProperty(ref publicKeyFile, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public string PrivateKeyFile
      {
         get => privateKeyFile;
         set
         {
            if (ValidateAndSetProperty(ref privateKeyFile, value))
            {
               OkCommand.RaiseCanExecuteChanged();
            }
         }
      }

      public bool ForceIPv4 { get; set; }

      public DelegateCommand BrowsePublicKeyFileCommand { get; }

      public DelegateCommand BrowsePrivateKeyFileCommand { get; }

      public DelegateCommand CreateKeyFileCommand { get; }

      public DelegateCommand<DialogWindow> OkCommand { get; }

      public DelegateCommand<DialogWindow> CancelCommand { get; }

      /// <summary>
      /// Gets a value that indicates whether the entity has validation errors.
      /// </summary>
      /// <seealso cref="INotifyDataErrorInfo.HasErrors"/>
      public bool HasErrors => validationErrors.Any();

      public IEnumerable GetErrors(string propertyName)
      {
         if (string.IsNullOrEmpty(propertyName))
         {
            return validationErrors.SelectMany(e => e.Value);
         }
         else
         {
            return validationErrors.TryGetValue(propertyName, out var errors) ? errors : new List<object>();
         }
      }

      public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

      private void HandleBrowsePublicKeyFileCommandCommand()
      {
         HandleBrowseKeyFileCommandCommand(".pub", "Public key files (.pub)|*.pub", (file) => PublicKeyFile = file);
      }

      private void HandleBrowsePrivateKeyFileCommandCommand()
      {
         HandleBrowseKeyFileCommandCommand("", "Private key files |*", (file) => PrivateKeyFile = file);
      }

      private void HandleBrowseKeyFileCommandCommand(string extension, string filterText, Action<string> setter)
      {
         var openFileDialog = new OpenFileDialog()
         {
            CheckFileExists = true,
            Multiselect = false,
            DefaultExt = extension,
            Filter = $"{filterText}| All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
         };

         var result = openFileDialog.ShowDialog();
         if (result.HasValue && result.Value)
         {
            setter(openFileDialog.FileName);
         }
      }

#pragma warning disable VSTHRD100 // Avoid async void methods
      private async void HandleCreateKeyFileCommandCommand()
      {
         // Show MessageBox to ask user which key type to create
         var messageBoxResult = System.Windows.MessageBox.Show(
            Resources.SetupSshDialogKeyTypePromptMessage,
            Resources.SetupSshDialogKeyTypePromptTitle,
            System.Windows.MessageBoxButton.YesNoCancel,
            System.Windows.MessageBoxImage.Question);

         SshKeyType keyType;
         if (messageBoxResult == System.Windows.MessageBoxResult.Yes)
         {
            keyType = SshKeyType.Rsa;
         }
         else if (messageBoxResult == System.Windows.MessageBoxResult.No)
         {
            keyType = SshKeyType.Ecdsa;
         }
         else
         {
            return; // User cancelled
         }

         bool result = await keyService.CreateAsync(keyType);
         if (result)
         {
            PublicKeyFile = keyService.GetPublicKeyPath(keyType);
            PrivateKeyFile = keyService.GetPrivateKeyPath(keyType);
         }
      }
#pragma warning restore VSTHRD100 // Avoid async void methods

      private void HandleOkCommand(DialogWindow dialog)
      {
         if (dialog != null)
         {
            ((IPasswordProvider)dialog).QueryPassword(out password);
            dialog.DialogResult = true;
            dialog.Close();
         }
      }

      private void HandleCancelCommand(DialogWindow dialog)
      {
         if (dialog != null)
         {
            dialog.DialogResult = false;
            dialog.Close();
         }
      }

      private void RaiseErrorsChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

      private bool ValidateAndSetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
      {
         // Clear previous errors of the current property to be validated 
         _ = ClearErrors(propertyName);

         bool valid = ValidateProperty(newValue, propertyName);

         _ = SetProperty(ref field, newValue, propertyName);

         return valid;
      }

      private bool ValidateProperty<T>(T newValue, string propertyName)
      {
         if (validationRules.TryGetValue(propertyName, out var propertyValidationRules))
         {
            var errors = propertyValidationRules.Select(validationRule => validationRule.Validate(newValue, CultureInfo.CurrentCulture))
              .Where(result => !result.IsValid)
              .Select(invalidResult => invalidResult.ErrorContent);

            AddErrors(propertyName, errors);

            return !errors.Any();
         }

         // No rules found for the current property
         return true;
      }

      private bool Validate()
      {
         bool Validate<T>(T newValue, string propertyName)
         {
            if (validationRules.TryGetValue(propertyName, out var propertyValidationRules))
            {
               return !propertyValidationRules.Select(validationRule => validationRule.Validate(newValue, CultureInfo.CurrentCulture)).Any(result => !result.IsValid);
            }

            return true;
         }

         bool valid = Validate(hostName, nameof(HostName));
         valid &= Validate(hostPort, nameof(HostPort));
         valid &= Validate(username, nameof(Username));
         valid &= Validate(publicKeyFile, nameof(PublicKeyFile));

         // also consider any existing binding/validation errors
         valid &= !HasErrors;

         return valid;
      }

      private void AddErrors(string propertyName, IEnumerable<object> newErrors)
      {
         if (!newErrors.Any())
         {
            return;
         }

         if (!validationErrors.TryGetValue(propertyName, out IList<object> propertyErrors))
         {
            propertyErrors = new List<object>();
            validationErrors.Add(propertyName, propertyErrors);
         }

         foreach (object error in newErrors)
         {
            propertyErrors.Insert(0, error);
         }

         RaiseErrorsChanged(propertyName);
      }

      /// <summary>
      /// Removes all errors of the specified property.Raises the ErrorsChanged event if the Errors collection changes. 
      /// </summary>
      /// <param name="propertyName">The property name to clear the errors</param>
      /// <returns></returns>
      private bool ClearErrors(string propertyName)
      {
         if (validationErrors.Remove(propertyName))
         {
            RaiseErrorsChanged(propertyName);
            return true;
         }
         return false;
      }

      /// <summary>
      /// Reports a binding conversion exception for the specified property to the view model's validation state.
      /// </summary>
      /// <param name="propertyName">The property name.</param>
      /// <param name="message">The error message.</param>
      public void ReportBindingException(string propertyName, string message)
      {
         // replace existing errors for the property with the provided message
         _ = ClearErrors(propertyName);
         AddErrors(propertyName, new[] { (object)message });
         OkCommand.RaiseCanExecuteChanged();
      }
   }
}