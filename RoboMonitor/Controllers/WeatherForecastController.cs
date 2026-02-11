using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        // 1. Definer Meter
        private static readonly Meter _weatherMeter = new("RoboMonitor.Weather", "1.0.0");

        // Variabel til at holde den seneste måling til vores Gauge
        private static double _lastMeasuredTemp;

        // 2. Definer instrumenter
        // Counter: Tæller totalt antal forespørgsler
        private static readonly Counter<int> _forecastRequestCounter =
            _weatherMeter.CreateCounter<int>("weather_requests_total", description: "Antal vejrudsigt-kald");

        // Histogram: God til at se gennemsnit og temperaturfordeling over tid
        private static readonly Histogram<double> _tempHistogram =
            _weatherMeter.CreateHistogram<double>("weather_temperature_histogram", unit: "Celsius");

        // ObservableGauge: Viser den "aktuelle" temperatur (det sidste punkt i listen)
        private static readonly ObservableGauge<double> _tempGauge =
            _weatherMeter.CreateObservableGauge("weather_current_temperature", () => _lastMeasuredTemp);

        private static readonly string[] Summaries = [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var forecasts = Enumerable.Range(1, 5).Select(index => {
                var temp = Random.Shared.Next(-20, 55);
                var summary = Summaries[Random.Shared.Next(Summaries.Length)];

                // Registrer temperaturen i vores Histogram
                _tempHistogram.Record(temp);

                // Tæl kaldet og gem hvilken type vejr der blev genereret som en "Label"
                _forecastRequestCounter.Add(1, new TagList { { "summary", summary } });

                return new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = temp,
                    Summary = summary
                };
            }).ToArray();

            // Opdater værdien for vores Gauge til det første element i den nye liste
            _lastMeasuredTemp = forecasts[0].TemperatureC;

            return forecasts;
        }
    }
}