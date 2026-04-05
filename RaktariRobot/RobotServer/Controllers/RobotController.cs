using Microsoft.AspNetCore.Mvc;
using RobotShared;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class RobotController : ControllerBase
    {
        // Ideiglenes memória a robot állapotának (később ide jön az Entity Framework adatbázis)
        private static RobotStateDto _robotState = new RobotStateDto
        {
            X = 0,
            Y = 0,
            Battery = 100,
            StateMessage = "Készenlét (Szerverről)"
        };

        // 1. HTTP GET (Állapot lekérdezése)
        [HttpGet("state")]
        public ActionResult<RobotStateDto> GetState()
        {
            return Ok(_robotState); // Automatikusan JSON-né alakítja a DTO-t!
        }

        // 2. HTTP POST (Parancs küldése - pl. Előre)
        [HttpPost("move-forward")]
        public ActionResult<RobotStateDto> MoveForward()
        {
            _robotState.Y += 1.5;
            _robotState.Battery -= 1;
            _robotState.StateMessage = "Mozgás előre...";

            return Ok(_robotState);
        }

        // 3. HTTP PUT (Vészmegállás - állapot felülírása)
        [HttpPut("emergency-stop")]
        public ActionResult<RobotStateDto> EmergencyStop()
        {
            _robotState.StateMessage = "VÉSZMEGÁLLÁS (Szerver regisztrálta)";
            return Ok(_robotState);
        }
    }
}