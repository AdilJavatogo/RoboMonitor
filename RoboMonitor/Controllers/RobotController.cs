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
                    RobotTask = "Levering", // ændre det til noget andet, måske "Ren"     
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
                    Lift = 457,
                    EStop = false,
                    ChargingTime = 15,
                    BreakCount = 200
                }
            };

        // SKAL LAVES FÆRDIGT
        private static TagList GetCommonTags(Robot robot) => new TagList
        {
            { "robot_id", robot.RobotId },
            { "hospital", robot.Hospital },
            { "department", robot.Department }
        };

        // 3. I den statiske constructor sætter vi vores målinger op, så de automatisk opdateres, når data i listen ændres
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

            // Måling: E-Stop som tal (0=OK, 1=Nødstop aktiveret)
            _robotMeter.CreateObservableGauge("robot_estop_code", () =>
            {
                return _robots.Select(robot => new Measurement<int>(
                    GetEStopCode(robot.EStop),
                    new TagList { { "robot_id", robot.RobotId } }
                ));
            });
        }

        // Hjælper til at lave status om til tal til grafer
        private static int GetStatusCode(string status) => status switch
        {
            "Online" => 1, 
            "Oplader" => 2,  
            "Offline" => 3,  
            _ => 0       
        };

        // Hjælper til at oversætte Robottilstand (State) til tal
        private static int GetStateCode(string state) => state switch
        {
            "Kører" => 1,   
            "Ledig" => 2,     
            "Oplader" => 3, 
            "Fejl" => 4,    
            _ => 0           
        };

        // Hjælper til at oversætte Robotopgave (Task) til tal
        private static int GetTaskCode(string task) => task switch
        {
            "Vaskning" => 1,
            "Levering" => 2,
            "Inspektion" => 3,
            "Ingen" => 4,    
            _ => 0           
        };

        // Hjælper til at oversætte Sensorstatus til tal
        private static int GetSensorCode(string sensor) => sensor switch
        {
            "OK" => 1,       
            "Advarsel" => 2,  
            "Fejl" => 3,    
            _ => 0           
        };

        // private static int GetEStopCode(bool isEStopActive) => isEStopActive ? 2 : 1;
        private static int GetEStopCode(bool sensor) => sensor switch
        {
            false => 0,   // OK
            true => 1,    // Nødstop aktiveret
        };

        [HttpGet(Name = "GetRobots")]
        public IEnumerable<Robot> Get()
        {
            return _robots;
        }

        // POST endpoint til at simulere ændringer i data
        [HttpPost("simulate")]
        public IActionResult SimulateData()
        {
            var rnd = Random.Shared;

            foreach (var robot in _robots)
            {
                // 1. Simuler CPU Temperatur
                double tempChange = rnd.NextDouble() * 4 - 2;
                robot.CPUTemperature = (int)Math.Clamp(robot.CPUTemperature + tempChange, 30.0, 90.0);

                // E-Stop har en lille chance for at blive aktiveret, og hvis den er aktiveret, er der en chance for at den deaktiveres igen
                if (rnd.Next(0, 100) == 99)
                {
                    robot.EStop = true;
                    robot.RobotState = "Fejl";
                    robot.RobotStatus = "Offline";
                    robot.SensorStatus = "Fejl";
                }
                else if (robot.EStop)
                {
                    if (rnd.Next(0, 100) > 80) robot.EStop = false;
                }

                // 2. Chance for at skifte tilstand
                if (!robot.EStop && rnd.Next(0, 10) > 7)
                {
                    string[] states = ["Ledig", "Kører", "Oplader", "Fejl"];
                    robot.RobotState = states[rnd.Next(states.Length)];
                }

                // 3. Opdater data baseret på den tilstand, robotten er i
                switch (robot.RobotState)
                {
                    case "Kører":
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - rnd.Next(1, 5), 0, 100);
                        robot.Distance += (int)Math.Round(rnd.NextDouble() * 10.0, 1);
                        robot.RobotStatus = "Online";
                        robot.SensorStatus = "OK";
                        robot.RobotTask = "Vaskning";

                        // Bremsetæller stiger
                        robot.BreakCount += rnd.Next(0, 3);

                        // Lift stiger lidt tilfældigt for at simulere, at robotten løfter ting
                        robot.Lift += rnd.Next(0, 2);

                        robot.ChargingTime = 0;

                        if (string.IsNullOrEmpty(robot.RobotTask) || robot.RobotTask == "Ingen")
                        {
                            string[] tasks = ["Vaskning", "Levering", "Inspektion"];
                            robot.RobotTask = tasks[rnd.Next(tasks.Length)];
                        }
                        break;

                    case "Oplader":
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel + rnd.Next(5, 15), 0, 100);
                        robot.RobotStatus = "Oplader";
                        robot.RobotTask = "Ingen";
                        robot.SensorStatus = "OK";
                        robot.ChargingTime += 5;

                        break;

                    case "Fejl":
                        robot.RobotStatus = "Offline";
                        robot.SensorStatus = "Fejl";
                        robot.RobotTask = "Ingen";
                        robot.ChargingTime = 0;
                        break;

                    case "Ledig":
                    default:
                        robot.BatteryLevel = Math.Clamp(robot.BatteryLevel - 1, 0, 100);
                        robot.RobotStatus = "Online";
                        robot.RobotTask = "Levering";
                        robot.SensorStatus = rnd.Next(0, 100) > 90 ? "Advarsel" : "OK";
                        robot.ChargingTime = 0;
                        break;
                }

                if (robot.BatteryLevel <= 0)
                {
                    robot.RobotState = "Fejl";
                    robot.RobotStatus = "Offline";
                    robot.RobotTask = "Ingen";
                }
            }

            return Ok(new { message = "Simulering udført: Alle robotdata er opdateret realistisk", data = _robots });
        }

        // Tilføj en robot manuelt via POST /robot/add?id=7
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
    }
}