using Microsoft.AspNetCore.Mvc;
using RobotShared;
using System.Collections.Generic;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/simulation")]
    public class SimulationController : ControllerBase
    {
        [HttpPost("tick")]
        public ActionResult<List<RobotDetailsDto>> Tick()
        {
            // Dummy implementation of the tick. Later, this should use the RobotSimulationService to actually move robots.
            // Currently, we just mock the return value. 

            return Ok(new List<RobotDetailsDto>()); 
        }

        [HttpPost("reset")]
        public ActionResult<SimulationResetResponseDto> Reset()
        {
            return Ok(new SimulationResetResponseDto
            {
                Success = true,
                Message = "Simulation reset completely.",
                Robots = new List<RobotDetailsDto>()
                // A Warehouse részt is itt kellene visszaadni (üres térkép, stb.)
            });
        }
    }
}