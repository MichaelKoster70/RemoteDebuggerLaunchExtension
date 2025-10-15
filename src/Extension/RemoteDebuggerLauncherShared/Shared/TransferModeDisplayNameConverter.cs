// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace RemoteDebuggerLauncher.Shared
{
   /// <summary>
   /// TypeConverter that shows DisplayAttribute.Name in the PropertyGrid drop-down and label,
   /// and maps back to the correct enum values.
   /// </summary>
   internal class TransferModeDisplayNameConverter : EnumConverter
   {
      public TransferModeDisplayNameConverter() : base(typeof(TransferMode))
      {
         //EMPTY_BODY
      }

      /// <summary>
      /// Gets the display name for the specified enum value using the DisplayAttribute, if present.
      /// </summary>
      /// <param name="value">The enum value to get the display name for.</param>
      /// <returns>The display name if available; otherwise, the enum name or value as string.</returns>
      private string GetDisplayName(object value)
      {
         if (value is null)
         {
            return string.Empty;
         }

         string name = Enum.GetName(EnumType, value);
         if (name is null)
         {
            return value.ToString();
         }

         var field = EnumType.GetField(name);
         var display = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));

         return (display != null && !string.IsNullOrEmpty(display.Name)) ? display.Name : name;
      }

      /// <summary>
      /// Attempts to parse a string to an enum value by matching the DisplayAttribute.Name.
      /// </summary>
      /// <param name="s">The display name string to parse.</param>
      /// <returns>The corresponding enum value if found; otherwise, null.</returns>
      private object TryParseFromDisplayName(string s)
      {
         foreach (var field in EnumType.GetFields().Where(f => f.IsStatic))
         {
            var display = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            if (display != null && string.Equals(display.Name, s, StringComparison.CurrentCulture))
            {
               return Enum.Parse(EnumType, field.Name);
            }
         }
         return null;
      }

      /// <inheritdoc />
      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
         return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
      }

      /// <inheritdoc />
      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
         return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
      }

      /// <inheritdoc />
      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
      {
         if (destinationType == typeof(string))
         {
            return GetDisplayName(value);
         }
         return base.ConvertTo(context, culture, value, destinationType);
      }

      /// <inheritdoc />
      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
         if (value is string s)
         {
            object display = TryParseFromDisplayName(s);
            if (display != null)
            {
               return display;
            }

            if (Enum.IsDefined(EnumType, s))
            {
               return Enum.Parse(EnumType, s, true);
            }
         }
         return base.ConvertFrom(context, culture, value);
      }

      /// <inheritdoc />
      public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

      /// <inheritdoc />
      public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

      /// <inheritdoc />
      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
         var values = Enum.GetValues(EnumType).Cast<object>()
             .OrderBy(v => GetDisplayName(v), StringComparer.CurrentCultureIgnoreCase)
             .ToArray();
         return new StandardValuesCollection(values);
      }
   }
}
