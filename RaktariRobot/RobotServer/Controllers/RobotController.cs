using Microsoft.AspNetCore.Mvc;
using RobotShared;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class RobotController : ControllerBase
    {
        // Ideiglenes memória a robot állapotának (késõbb ide jön az Entity Framework adatbázis)
        private static RobotDetailsDto _robotState = new RobotDetailsDto
        {
            RobotId = "ROBOT-001",
            IsOnline = true,
            State = RobotState.Ready,
        };

        // 1. HTTP GET (Állapot lekérdezése)
        [HttpGet("state")]
        public ActionResult<RobotDetailsDto> GetState()
        {
            return Ok(_robotState); // Automatikusan JSON-né alakítja a DTO-t!
        }

        // 2. HTTP POST (Parancs küldése - pl. Elõre)
        [HttpPost("move-forward")]
        public ActionResult<RobotDetailsDto> MoveForward()
        {
            _robotState.Position.Y += 1.5f;
            _robotState.BatteryLevel -= 1;

            return Ok(_robotState);
        }

        // 3. HTTP PUT (Vészmegállás - állapot felülírása)
        [HttpPut("emergency-stop")]
        public ActionResult<RobotDetailsDto> EmergencyStop()
        {
            _robotState.State = RobotState.EmergencyStop;
            return Ok(_robotState);
        }
    }
}


