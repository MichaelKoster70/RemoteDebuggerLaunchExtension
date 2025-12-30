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
         if (value is int intValue)
         {
            if (intValue < Min || intValue > Max)
            {
               return new ValidationResult(false, $"Port number must be between {Min} and {Max}.");
            }
            return ValidationResult.ValidResult;
         }

         if (value is string text && string.IsNullOrEmpty(text))
         {
            // empty string is invalid so OK can't be enabled
            return new ValidationResult(false, "Port number is required.");
         }

         return new ValidationResult(false, "Invalid port number.");
      }
   }
}
