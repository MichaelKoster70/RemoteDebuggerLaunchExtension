// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace WebBlazorServerApp.Data
{
   public class WeatherForecastService
   {
      private static readonly string[] Summaries = new[]
      {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

      public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
      {
#pragma warning disable CA5394 // no productive code
         return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
         {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
         }).ToArray());
#pragma warning restore CA5394
      }
   }
}