// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

namespace WebBlazorServerApp.Data;

public class WeatherForecastService
{
   private static readonly string[] summaries =
   [
     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
   ];

   public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
   {
      return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
      {
         Date = startDate.AddDays(index),
         TemperatureC = Random.Shared.Next(-20, 55),
         Summary = summaries[Random.Shared.Next(summaries.Length)]
      }).ToArray());
   }
}