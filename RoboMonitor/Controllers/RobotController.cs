using Microsoft.AspNetCore.Mvc;

namespace RoboMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RobotController : Controller
    {
        // Batteriniveau i procent
        // Afstand i meter
        // Sensorstatus (OK, Warning, Error)
        // CPU-temperatur i grader Celsius
        // Robottilstand (Idle, Moving, Charging, Error) - (Grøn = Kører, Gul = Oplader, Rød = Fejl/Offline).
        // Robotopgaver ("Vaskning", "Levering")

        // Robot klasse til at holde robotdata
    }
}
