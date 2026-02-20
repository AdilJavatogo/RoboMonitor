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
                    Hospital = "OUH",
                    Department = "Akut Modtagelsen",
                    BatteryLevel = 85,
                    RobotStatus = "Online",
                    RobotState = "Kører",
                    RobotTask = "Vaskning",      
                    SensorStatus = "OK",         
                    Distance = 125,            
                    CPUTemperature = 45,
                    Lift = 125,
                    EStop = false,
                    ChargingTime = 12,
                    BreakCount = 120
                },
                new Robot
                {
                    RobotId = 2,
                    Hospital = "OUH",
                    Department = "Kardiologisk",
                    BatteryLevel = 85,
                    RobotStatus = "Online",
                    RobotState = "Kører",
                    RobotTask = "Vaskning",
                    SensorStatus = "OK",
                    Distance = 125,
                    CPUTemperature = 45,
                    Lift = 329,
                    EStop = false,
                    ChargingTime = 12,
                    BreakCount = 120
                },
                new Robot
                {
                    RobotId = 3,
                    Hospital = "Rigshospitalet",
                    Department = "Kardiologisk",
                    BatteryLevel = 42,
                    RobotStatus = "Oplader",
                    RobotState = "Oplader",
                    RobotTask = "Ingen",        
                    SensorStatus = "Advarsel",    
                    Distance = 0,              
                    CPUTemperature = 38,
                    Lift = 13,
                    EStop = true,
                    ChargingTime = 30,
                    BreakCount = 50
                },
                 new Robot
                {
                    RobotId = 4,
                    Hospital = "Herlev Hospital",
                    Department = "Kardiologisk",
                    BatteryLevel = 42,
                    RobotStatus = "Oplader",
                    RobotState = "Oplader",
                    RobotTask = "Ingen",
                    SensorStatus = "Advarsel",
                    Distance = 0,
                    CPUTemperature = 38,
                    Lift = 56,
                    EStop = true,
                    ChargingTime = 30,
                    BreakCount = 50
                },
                new Robot
                {
                    RobotId = 5,
                    Hospital = "Herlev Hospital",
                    Department = "Onkologisk",
                    BatteryLevel = 12,
                    RobotStatus = "Offline",
                    RobotState = "Fejl",
                    RobotTask = "Levering",      
                    SensorStatus = "Fejl",      
                    Distance = 1050,           
                    CPUTemperature = 65,
                    Lift = 594,
                    EStop = false,
                    ChargingTime = 15,
                    BreakCount = 200
                },
                 new Robot
                {
                    RobotId = 6,
                    Hospital = "Herlev Hospital",
                    Department = "Onkologisk",
                    BatteryLevel = 12,
                    RobotStatus = "Offline",
                    RobotState = "Fejl",
                    RobotTask = "Levering",
                    SensorStatus = "Fejl",
                    Distance = 1050,
                    CPUTemperature = 65,
                    Lift = 39,
                    EStop = false,
                    ChargingTime = 15,
                    BreakCount = 200
                }
            };

        // 3. Vi opretter ObservableGauges
        // De kører automatisk hver gang Prometheus "skraber" data (hvert 5. sek)
        static RobotController()
        {
            // Måling: Batteriniveau
            _robotMeter.CreateObservableGauge("robotfleet", () =>
            {
                // Vi returnerer en måling for hver robot i listen med dens ID som label
                return _robots.Select(robot => new Measurement<int>(
                    robot.BatteryLevel,
                    new TagList {
                        { "robot_id", robot.RobotId },
                        { "hospital", robot.Hospital },    
                        { "department", robot.Department },
                        { "status_text", robot.RobotStatus }, 
                        { "state", robot.RobotState },
                        { "task", robot.RobotTask},
                        { "sensor", robot.SensorStatus },
                        { "temperature", robot.CPUTemperature },
                        { "lift", robot.Lift },
                        { "estop", robot.EStop },
                        { "charging_time", robot.ChargingTime },
                        { "break_count", robot.BreakCount }
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

            // Måling: Tilstand (State) som tal til State Timeline
            _robotMeter.CreateObservableGauge("robot_state_code", () =>
            {
                return _robots.Select(robot => new Measurement<int>(
                    GetStateCode(robot.RobotState),
                    new TagList { { "robot_id", robot.RobotId } }
                ));
            });

            // Måling: Opgave (Task) som tal til State Timeline
            _robotMeter.CreateObservableGauge("robot_task_code", () =>
            {
                return _robots.Select(robot => new Measurement<int>(
                    GetTaskCode(robot.RobotTask),
                    new TagList { { "robot_id", robot.RobotId } }
                ));
            });

            // Måling: Sensorstatus som tal til State Timeline
            _robotMeter.CreateObservableGauge("robot_sensor_code", () =>
            {
                return _robots.Select(robot => new Measurement<int>(
                    GetSensorCode(robot.SensorStatus),
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

        // Hjælper til at oversætte Robottilstand (State) til tal
        private static int GetStateCode(string state) => state switch
        {
            "Kører" => 1,   // Kører aktivt
            "Ledig" => 2,     // Venter/Standby
            "Oplader" => 3, // Lader op
            "Fejl" => 4,    // Fejltilstand
            _ => 0           // Ukendt
        };

        // Hjælper til at oversætte Robotopgave (Task) til tal
        private static int GetTaskCode(string task) => task switch
        {
            "Vaskning" => 1,
            "Levering" => 2,
            "Inspektion" => 3,
            "Ingen" => 4,    // Ingen aktiv opgave
            _ => 0           // Ukendt
        };

        // Hjælper til at oversætte Sensorstatus til tal
        private static int GetSensorCode(string sensor) => sensor switch
        {
            "OK" => 1,       // Alt fungerer (Grøn)
            "Advarsel" => 2,  // Advarsel, f.eks. snavset sensor (Gul)
            "Fejl" => 3,    // Fejl, f.eks. blokeret (Rød)
            _ => 0           // Ukendt
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
                // 1. Simuler CPU Temperatur (eksisterende logik)
                double tempChange = rnd.NextDouble() * 4 - 2;
                robot.CPUTemperature = (int)Math.Clamp(robot.CPUTemperature + tempChange, 30.0, 90.0);

                // ---------------------------------------------------------
                // NYT: E-Stop logik
                // ---------------------------------------------------------
                // 1% chance for at nogen trykker på Nødstop
                if (rnd.Next(0, 100) == 99)
                {
                    robot.EStop = true;
                    robot.RobotState = "Fejl"; // Nødstop tvinger robotten i fejl
                    robot.RobotStatus = "Offline";
                    robot.SensorStatus = "Fejl";
                }
                else if (robot.EStop)
                {
                    // Hvis E-Stop er aktiv, er der 20% chance for at det bliver løst (reset)
                    if (rnd.Next(0, 100) > 80) robot.EStop = false;
                }

                // 2. Chance for at skifte tilstand (kun hvis E-Stop IKKE er aktiv)
                if (!robot.EStop && rnd.Next(0, 10) > 7)
                {
                    string[] states = ["Ledig", "Kører", "Oplader", "Fejl"];
                    robot.RobotState = states[rnd.Next(states.Length)];
                }

                // 3. Opdater data baseret på den tilstand, robotten er i
                switch (robot.RobotState)
                {
                    case "Kører":
                        // Eksisterende: Strøm, distance, status
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - rnd.Next(1, 5), 0, 100);
                        robot.Distance += (int)Math.Round(rnd.NextDouble() * 10.0, 1);
                        robot.RobotStatus = "Online";
                        robot.SensorStatus = "OK";
                        robot.RobotTask = "Vaskning";

                        // NYT: Bremsetæller stiger når den kører (0, 1 eller 2 opbremsninger)
                        robot.BreakCount += rnd.Next(0, 3);

                        // NYT: Liften bruges ofte under kørsel (50% chance)
                        robot.Lift = rnd.Next(0, 10) > 5;

                        // NYT: Vi lader ikke når vi kører
                        robot.ChargingTime = 0;

                        // Tildel opgave
                        if (string.IsNullOrEmpty(robot.RobotTask) || robot.RobotTask == "Ingen")
                        {
                            string[] tasks = ["Vaskning", "Levering", "Inspektion"];
                            robot.RobotTask = tasks[rnd.Next(tasks.Length)];
                        }
                        break;

                    case "Oplader":
                        // Eksisterende: Får strøm
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel + rnd.Next(5, 15), 0, 100);
                        robot.RobotStatus = "Oplader";
                        robot.RobotTask = "Ingen";
                        robot.SensorStatus = "OK";

                        // NYT: Tæl ladetid op (simulerer at der går tid)
                        robot.ChargingTime += 5;

                        // NYT: Liften er nede under opladning
                        robot.Lift = false;
                        break;

                    case "Fejl":
                        robot.RobotStatus = "Offline";
                        robot.SensorStatus = "Fejl";
                        robot.RobotTask = "Ingen";

                        // NYT: Ingen ladning under fejl
                        robot.ChargingTime = 0;
                        break;

                    case "Ledig":
                    default:
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - 1, 0, 100);
                        robot.RobotStatus = "Online";
                        robot.RobotTask = "Levering";
                        robot.SensorStatus = rnd.Next(0, 100) > 90 ? "Advarsel" : "OK";

                        // NYT: Reset ladetid hvis vi bare står stille (ikke lader)
                        robot.ChargingTime = 0;
                        break;
                }

                // Sikkerhedsnet: Hvis batteriet dør helt
                if (robot.BatteryLevel <= 0)
                {
                    robot.RobotState = "Fejl";
                    robot.RobotStatus = "Offline";
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
                RobotStatus = "Online",
                RobotState = "Ledig"
            });

            return Ok($"Robot {id} tilføjet! Den dukker op i Grafana om ca. 5 sekunder.");
        }
    }
}