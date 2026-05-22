using Microsoft.AspNetCore.Mvc;
using RobotServer.Services;
using RobotShared;
using System.Collections.Generic;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/simulation")]
    public class SimulationController : ControllerBase
    {
        private readonly RobotSimulationService _simulationService;

        public SimulationController(RobotSimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpPost("tick")]
        public ActionResult<List<RobotDetailsDto>> Tick()
        {
            var robots = _simulationService.Tick();
            return Ok(robots);
        }

        [HttpPost("reset")]
        public ActionResult<SimulationResetResponseDto> Reset()
        {
            var result = _simulationService.ResetSimulation();
            return Ok(result);
        }
    }
}