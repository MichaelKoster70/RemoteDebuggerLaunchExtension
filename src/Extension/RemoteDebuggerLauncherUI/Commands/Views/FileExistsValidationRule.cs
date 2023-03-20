// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace RemoteDebuggerLauncher
{
   internal class FileExistsValidationRule : ValidationRule
   {
      public FileExistsValidationRule(string errorText = "")
      {
         ErrorText = errorText;
      }

      public string ErrorText { get; set; }

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         string text = value as string;
         if (!string.IsNullOrEmpty(text) && File.Exists(text))
         {
            return ValidationResult.ValidResult;
         }

         return new ValidationResult(false, ErrorText);
      }
   }
}
