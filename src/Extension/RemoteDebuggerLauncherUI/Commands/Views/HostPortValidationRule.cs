// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Globalization;
using System.Windows.Controls;

namespace RemoteDebuggerLauncher
{
   internal class HostPortValidationRule : ValidationRule
   {
      public HostPortValidationRule(int min = 1, int max = 65535)
      {
         Min = min;
         Max = max;
      }

      public int Min { get; set; }
      public int Max { get; set; }

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         string text = value as string;
         if (string.IsNullOrEmpty(text))
         {
            // an empty string is considered valid
            return ValidationResult.ValidResult;
         }



         return ValidationResult.ValidResult;
      }
   }
}
