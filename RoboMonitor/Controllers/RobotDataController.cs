using Microsoft.AspNetCore.Mvc;
using RoboMonitor.Models;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RobotDataController : Controller
    {
        //[HttpPost]
        //public IActionResult PostRobotData([FromBody] Robot robot)
        //{
        //    // Prometheus data modtaget - opdater robotten i listen


        //    Console.WriteLine($"Data mnodtaget: {robot.BatteryLevel}"); // robot.Id osv.

        //    return Ok();
        //}

        [HttpPost]
        public IActionResult PostData([FromBody] Robot robot)
        {
            var robotList = RobotController._robots.FirstOrDefault(r => r.RobotId == robot.RobotId);

            if (robotList != null)
            {
                // OPDATERE Grafana via den anden liste  - DETTE SKAL NOK LAVES TIL SIN EGEN LISTE SENERE
                robotList.BatteryLevel = robot.BatteryLevel;
                robotList.CPUTemperature = robot.CPUTemperature;
                robotList.RobotStatus = robot.RobotStatus;
                robotList.ChargingTime = robot.ChargingTime;
                robotList.EStop = robot.EStop;
                robotList.Lift = robot.Lift;
                robotList.BreakCount = robot.BreakCount;
                robotList.Department = robot.Department;
                robotList.Distance = robot.Distance;

                Console.WriteLine($"[UPDATE] Robot {robot.RobotId}: Batteri {robot.BatteryLevel}%");
            }
            else
            {
                // Tilføj ny robot til listen
                RobotController._robots.Add(robot);
                Console.WriteLine($"ID {robot.RobotId} er nu registreret i systemet!");
            }

            return Ok();
        }
    }
}
