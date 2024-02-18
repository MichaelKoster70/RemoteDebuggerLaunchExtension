// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using WebBlazorWasmApp.Shared;

namespace WebBlazorWasmApp.Server.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
   {
      private static readonly string[] summaries =
      [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

      private readonly ILogger<WeatherForecastController> logger = logger;

      [HttpGet]
      public IEnumerable<WeatherForecast> Get()
      {
         return Enumerable.Range(1, 5).Select(index => new WeatherForecast
         {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
         })
         .ToArray();
      }
   }
}