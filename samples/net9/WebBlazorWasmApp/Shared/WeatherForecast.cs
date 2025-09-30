// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace WebBlazorWasmApp.Shared
{
   public class WeatherForecast
   {
      public DateTime Date { get; set; }

      public int TemperatureC { get; set; }

      public string? Summary { get; set; }

      public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
   }
}