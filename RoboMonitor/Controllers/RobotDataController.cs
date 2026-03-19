using Microsoft.AspNetCore.Mvc;
using RoboMonitor.Models;
using RoboMonitor.Repositories;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RobotDataController : Controller
    {

        private readonly IRobotRepository _repository;

        public RobotDataController(IRobotRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public IActionResult PostData([FromBody] Robot robot)
        {
            _repository.UpsertRobot(robot);

            Console.WriteLine($"[UPDATE] Robot {robot.RobotId}: Batteri {robot.BatteryLevel}%, State: {robot.RobotState}");
            return Ok();
        }

        [HttpGet(Name = "GetRobots")]
        public IEnumerable<Robot> Get()
        {
            return _repository.GetAllRobots();
        }


        //[HttpPost]
        //public IActionResult PostData([FromBody] Robot robot)
        //{
        //    var robotList = RobotController._robots.FirstOrDefault(r => r.RobotId == robot.RobotId);

        //    if (robotList != null)
        //    {
        //        // OPDATERE Grafana via den anden liste  - DETTE SKAL NOK LAVES TIL SIN EGEN LISTE SENERE
        //        robotList.BatteryLevel = robot.BatteryLevel;
        //        robotList.CPUTemperature = robot.CPUTemperature;
        //        robotList.RobotStatus = robot.RobotStatus;
        //        robotList.ChargingTime = robot.ChargingTime;
        //        robotList.EStop = robot.EStop;
        //        robotList.Lift = robot.Lift;
        //        robotList.BreakCount = robot.BreakCount;
        //        robotList.Department = robot.Department;
        //        robotList.Distance = robot.Distance;

        //        Console.WriteLine($"[UPDATE] Robot {robot.RobotId}: Batteri {robot.BatteryLevel}%");
        //    }
        //    else
        //    {
        //        // Tilføj ny robot til listen
        //        RobotController._robots.Add(robot);
        //        Console.WriteLine($"ID {robot.RobotId} er nu registreret i systemet!");
        //    }

        //    return Ok();
        //}
    }
}
