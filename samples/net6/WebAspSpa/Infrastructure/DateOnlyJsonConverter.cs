// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Text.Json.Serialization;
using System.Text.Json;

#nullable enable

namespace WebAspSpa.Infrastructure
{
   public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
   {
      public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
         return DateOnly.FromDateTime(reader.GetDateTime());
      }

      public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
      {
         _ = writer ?? throw new ArgumentNullException(nameof(writer));
         var isoDate = value.ToString("O");
         writer.WriteStringValue(isoDate);
      }
   }
}
