using Microsoft.AspNetCore.Mvc;
using RoboMonitor.Models;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RobotController : ControllerBase
    {
        // 1. Vi opretter et Meter specifikt til Robotter
        private static readonly Meter _robotMeter = new("RoboMonitor.Robots", "1.0.0");

        // 2. Vi laver en statisk liste til at simulere en database i hukommelsen
        private static readonly List<Robot> _robots = new()
            {
                new Robot
                {
                    RobotId = 1,
                    BatteryLevel = 85,
                    RobotStatus = "Online",
                    RobotState = "Moving",
                    RobotTask = "Vaskning",      
                    SensorStatus = "OK",         
                    Distance = 125.5,            
                    CPUTemperature = 45       
                },
                new Robot
                {
                    RobotId = 2,
                    BatteryLevel = 42,
                    RobotStatus = "Oplader",
                    RobotState = "Charging",
                    RobotTask = "Ingen",        
                    SensorStatus = "Warning",    
                    Distance = 0.0,              
                    CPUTemperature = 38        
                },
                new Robot
                {
                    RobotId = 3,
                    BatteryLevel = 12,
                    RobotStatus = "Offline",
                    RobotState = "Error",
                    RobotTask = "Levering",      
                    SensorStatus = "Error",      
                    Distance = 1050.2,           
                    CPUTemperature = 65        
                }
            };

        // 3. Vi opretter ObservableGauges
        // De kører automatisk hver gang Prometheus "skraber" data (hvert 5. sek)
        static RobotController()
        {
            // Måling: Batteriniveau
            _robotMeter.CreateObservableGauge("robot_battery_level", () =>
            {
                // Vi returnerer en måling for hver robot i listen med dens ID som label
                return _robots.Select(robot => new Measurement<int>(
                    robot.BatteryLevel,
                    new TagList {
                        { "robot_id", robot.RobotId },
                        { "status_text", robot.RobotStatus }, // Vi sender status med som tekst-label
                        { "state", robot.RobotState },
                        { "task", robot.RobotTask},
                        { "sensor", robot.SensorStatus },
                        { "temperature", robot.CPUTemperature }
                    }));
            });

            // Måling: Status som tal (nemmere at lave grafer på: 1=Grøn, 2=Gul, 3=Rød)
            _robotMeter.CreateObservableGauge("robot_status_code", () =>
            {
                return _robots.Select(robot => new Measurement<int>(
                    GetStatusCode(robot.RobotStatus),
                    new TagList { { "robot_id", robot.RobotId } }
                ));
            });
        }

        // Hjælper til at lave status om til tal til grafer
        private static int GetStatusCode(string status) => status switch
        {
            "Online" => 1, // Alt OK
            "Oplader" => 2,  // Advarsel
            "Offline" => 3,  // Fejl
            _ => 0       // Ukendt
        };

        [HttpGet(Name = "GetRobots")]
        public IEnumerable<Robot> Get()
        {
            return _robots;
        }

        [HttpPost("update/{id}")]
        public IActionResult UpdateRobot(int id, [FromBody] Robot inputData)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == id);
            if (robot == null) return NotFound();

            // Opdater felterne
            robot.BatteryLevel = inputData.BatteryLevel;
            robot.RobotStatus = inputData.RobotStatus;
            robot.RobotState = inputData.RobotState;

            // De nye felter
            robot.RobotTask = inputData.RobotTask;
            robot.SensorStatus = inputData.SensorStatus;
            robot.Distance = inputData.Distance;
            robot.CPUTemperature = inputData.CPUTemperature;

            return Ok(new { message = $"Robot {id} manuelt opdateret", data = robot });
        }

        // POST endpoint til at simulere ændringer i data
        [HttpPost("simulate")]
        public IActionResult SimulateData()
        {
            var rnd = Random.Shared;

            foreach (var robot in _robots)
            {
                // 1. Simuler CPU Temperatur (svinger lidt op og ned med +/- 2 grader)
                double tempChange = rnd.NextDouble() * 4 - 2;
                robot.CPUTemperature = (int)Math.Clamp(robot.CPUTemperature + tempChange, 30.0, 90.0);

                // 2. Chance for at skifte tilstand (f.eks. 20% chance hver gang man trykker)
                if (rnd.Next(0, 10) > 7)
                {
                    string[] states = ["Idle", "Moving", "Charging", "Error"];
                    robot.RobotState = states[rnd.Next(states.Length)];
                }

                // 3. Opdater data baseret på den tilstand, robotten er i
                switch (robot.RobotState)
                {
                    case "Moving":
                        // Når den kører: Bruger strøm, øger distance, status er Grøn
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - rnd.Next(1, 5), 0, 100);
                        robot.Distance += Math.Round(rnd.NextDouble() * 10.0, 1); // Kører 0-10 meter
                        robot.RobotStatus = "Online";
                        robot.SensorStatus = "OK";
                        robot.RobotTask = "Vaskning";

                        // Tildel en opgave hvis den ikke har en
                        if (robot.RobotTask == "Ingen" || robot.RobotTask == null)
                        {
                            string[] tasks = ["Vaskning", "Levering", "Inspektion"];
                            robot.RobotTask = tasks[rnd.Next(tasks.Length)];
                        }
                        break;

                    case "Charging":
                        // Når den lader: Får strøm, status er Gul
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel + rnd.Next(5, 15), 0, 100);
                        robot.RobotStatus = "Oplader";
                        robot.RobotTask = "Ingen"; // Man arbejder ikke når man lader
                        robot.SensorStatus = "OK";
                        break;

                    case "Error":
                        // Ved fejl: Status er Rød
                        robot.RobotStatus = "Offline";
                        robot.SensorStatus = "Error";
                        robot.RobotTask = "Ingen";
                        break;

                    case "Idle":
                    default:
                        // Standby: Bruger lidt strøm
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - 1, 0, 100);
                        robot.RobotStatus = "Online";
                        robot.RobotTask = "Levering";

                        // Lille chance for sensor warning i idle
                        robot.SensorStatus = rnd.Next(0, 100) > 90 ? "Warning" : "OK";
                        break;
                }

                // Sikkerhedsnet: Hvis batteriet dør helt
                if (robot.BatteryLevel <= 0)
                {
                    robot.RobotState = "Error";
                    robot.RobotStatus = "Rød";
                    robot.RobotTask = "Ingen";
                }
            }

            return Ok(new { message = "Simulering udført: Alle robotdata er opdateret realistisk", data = _robots });
        }
        // POST endpoint til at tilføje en ny robot dynamisk
        [HttpPost("add")]
        public IActionResult AddRobot(int id)
        {
            if (_robots.Any(r => r.RobotId == id))
                return BadRequest("Robot ID findes allerede");

            _robots.Add(new Robot
            {
                RobotId = id,
                BatteryLevel = 100,
                RobotStatus = "Grøn",
                RobotState = "Idle"
            });

            return Ok($"Robot {id} tilføjet! Den dukker op i Grafana om ca. 5 sekunder.");
        }
    }
}