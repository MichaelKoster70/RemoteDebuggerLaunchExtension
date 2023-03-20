// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RemoteDebuggerLauncher
{
   internal class HostNameValidationRule : ValidationRule
   {
      private const string regex = @"^(?!.{256})(?:[a-zA-Z0-9-]{1,63}\.){1,126}[a-zA-Z0-9]{2,}$|^[a-zA-Z0-9][a-zA-Z0-9-]{1,61}[a-zA-Z0-9]$";

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         string text = value as string;

         // an empty sting is not valid
         bool valid = !string.IsNullOrEmpty(text);
         if (!valid)
         {
            return new ValidationResult(false, Resources.HostNameValidationRuleNoValidNameOrIp);
         }

         // try if it validates as IPv4
         valid = IPAddress.TryParse(text, out _);
         
         if (!valid)
         {
            valid = Regex.IsMatch(text, regex);
         }

         return valid ? ValidationResult.ValidResult : new ValidationResult(false, Resources.HostNameValidationRuleNoValidNameOrIp);
      }
   }
}
