// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace RemoteDebuggerLauncher
{
   /// <summary>
   /// Class intended to hold the encryption/decription logic needed to store password in a launchsettings.json file.
   /// Implements the <see cref="ILaunchProfileExtensionValueProvider" /> interface
   /// </summary>
   [ExportLaunchProfileExtensionValueProvider("password", ExportLaunchProfileExtensionValueProviderScope.LaunchProfile)]
   public sealed class PasswordPropertyValueProvider : ILaunchProfileExtensionValueProvider
   {
      public string OnGetPropertyValue(string propertyName, ILaunchProfile launchProfile, ImmutableDictionary<string, object> globalSettings, Rule rule)
      {
         switch (propertyName)
         {
            case SecureShellRemoteLaunchProfile.passwordProperty:
               return GetEncryptedOtherProperty(launchProfile, propertyName, String.Empty);
            default:
               throw new InvalidOperationException($"{nameof(PasswordPropertyValueProvider)} does not handle property '{propertyName}'.");
         }
      }

      public void OnSetPropertyValue(string propertyName, string propertyValue, IWritableLaunchProfile launchProfile, ImmutableDictionary<string, object> globalSettings, Rule rule)
      {
         switch (propertyName)
         {
            case SecureShellRemoteLaunchProfile.passwordProperty:
               TrySetEncryptedOtherProperty(launchProfile, propertyName, propertyValue, string.Empty);
               break;
            default:
               throw new InvalidOperationException($"{nameof(PasswordPropertyValueProvider)} does not handle property '{propertyName}'.");
         }
      }

      private static string GetEncryptedOtherProperty(ILaunchProfile launchProfile, string propertyName, string defaultValue)
      {
         if (launchProfile.OtherSettings is null)
         {
            return defaultValue;
         }

         if (launchProfile.OtherSettings.TryGetValue(propertyName, out object rawValue))
         {
            if (rawValue is string encryptedValue && !String.IsNullOrEmpty(encryptedValue))
            {
               try
               {
                  var encryptedBytes = Convert.FromBase64String(encryptedValue);
                  var valueBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                  return Encoding.Unicode.GetString(valueBytes);
               }
               catch (Exception)
               {
                  // no mather what failed (base 64 decoding, decrypting or converting back to string), treat value as unset
               }
            }
         }

         return defaultValue;
      }

      private static bool TrySetEncryptedOtherProperty(IWritableLaunchProfile launchProfile, string propertyName, string value, string defaultValue)
      {
         if (!launchProfile.OtherSettings.TryGetValue(propertyName, out object current))
         {
            current = defaultValue;
         }

         var valueBytes = Encoding.Unicode.GetBytes(value);
         var encryptedBytes = ProtectedData.Protect(valueBytes, null, DataProtectionScope.CurrentUser);
         var encrypteValue = Convert.ToBase64String(encryptedBytes);

         bool replace = current is string;
         if (replace)
         {
            var currentValue = current as string;
            replace = !String.Equals(currentValue, encrypteValue);
         }

         if (replace)
         {
            launchProfile.OtherSettings[propertyName] = encrypteValue;
            return true;
         }

         return false;
      }
   }
}
