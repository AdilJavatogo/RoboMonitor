using Microsoft.AspNetCore.Mvc;
using System.Diagnostics; // Vigtigt for Activity

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Hent den nuværende "span" (activity)
            var activity = Activity.Current;

            if (activity != null)
            {
                // Tilføj en simpel værdi (Tag)
                activity.SetTag("forecast.generated.count", forecasts.Length);

                // Tilføj selve dataene som Events, så de kan ses i konsollen
                foreach (var f in forecasts)
                {
                    activity.AddEvent(new ActivityEvent("WeatherDataPoint", tags: new ActivityTagsCollection
                    {
                        { "date", f.Date },
                        { "temp", f.TemperatureC },
                        { "summary", f.Summary }
                    }));
                }
            }

            return forecasts;
        }
    }
}