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
    }
}
