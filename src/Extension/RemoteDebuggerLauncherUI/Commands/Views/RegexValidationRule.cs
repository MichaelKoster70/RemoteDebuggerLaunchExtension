// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RemoteDebuggerLauncher
{
   internal class RegexValidationRule : ValidationRule
   {
      public RegexValidationRule(string validateRegex, string errorText)
      {
         ValidateRegex = validateRegex;
         ErrorText = errorText;
      }

      public string ValidateRegex { get; set; }
      public string ErrorText { get; set; }

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         string text = value as string;

         // an empty sting is not valid
         if (string.IsNullOrEmpty(text))
         {
            return new ValidationResult(false, ErrorText);
         }

         bool valid = Regex.IsMatch(text, ValidateRegex);

         return valid ? ValidationResult.ValidResult : new ValidationResult(false, ErrorText);
      }
   }
}
