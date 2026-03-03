using Microsoft.AspNetCore.Mvc;
using RoboMonitor.Models;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RobotDataController : Controller
    {
        [HttpPost]
        public IActionResult PostRobotData([FromBody] Robot robot)
        {
            // Her kan du tilføje logik til at gemme eller behandle robotdataen
            // For eksempel, gemme dataen i en database eller sende den til en anden service
            // Returner en succesrespons

            Console.WriteLine($"Data mnodtaget: {robot.BatteryLevel}"); // robot.Id osv.

            return Ok();
        }
    }
}
