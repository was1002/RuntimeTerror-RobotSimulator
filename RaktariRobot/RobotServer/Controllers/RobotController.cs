using Microsoft.AspNetCore.Mvc;
using RobotServer.Services;
using RobotShared;
using System.Collections.Generic;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/robots")]
    public class RobotController : ControllerBase
    {
        private readonly RobotSimulationService _simulationService;

        public RobotController(RobotSimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpGet]
        public ActionResult<List<RobotDetailsDto>> GetAllRobots()
        {
            return Ok(_simulationService.GetRobots());
        }

        [HttpGet("statistics")]
        public ActionResult<RobotStatisticsDto> GetRobotStatistics()
        {
            var statistics = _simulationService.GetRobotStatistics();

            return Ok(statistics);
        }

        [HttpPost]
        public ActionResult<RobotCommandResultDto> CreateRobot([FromBody] CreateRobotRequestDto request)
        {
            var result = _simulationService.CreateRobot(request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/rename")]
        public ActionResult<RobotCommandResultDto> RenameRobot(int robotId, [FromBody] RenameRobotRequestDto request)
        {
            var result = _simulationService.RenameRobot(robotId, request);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/resume")]
        public ActionResult<RobotCommandResultDto> ResumeRobot(int robotId)
        {
            var result = _simulationService.ResumeRobot(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/pause")]
        public ActionResult<RobotCommandResultDto> PauseRobot(int robotId)
        {
            var result = _simulationService.PauseRobot(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/move-to-charger")]
        public ActionResult<RobotCommandResultDto> MoveToCharger(int robotId)
        {
            var result = _simulationService.MoveToCharger(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/move-to-service")]
        public ActionResult<RobotCommandResultDto> MoveToService(int robotId)
        {
            var result = _simulationService.MoveToService(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/move-to-location")]
        public ActionResult<RobotCommandResultDto> MoveToLocation(int robotId, [FromBody] MoveToLocationRequestDto request)
        {
            var result = _simulationService.MoveToLocation(robotId, request);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/self-test")]
        public ActionResult<SelfTestResultDto> RunSelfTest(int robotId)
        {
            var result = _simulationService.RunSelfTest(robotId);

            if (!result.RobotExists)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost("{robotId}/clear-warning")]
        public ActionResult<RobotCommandResultDto> ClearWarning(int robotId)
        {
            var result = _simulationService.ClearWarning(robotId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/fix-error")]
        public ActionResult<RobotCommandResultDto> FixError(int robotId)
        {
            var result = _simulationService.FixError(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("{robotId}/simulate-fault")]
        public ActionResult<RobotCommandResultDto> SimulateFault(int robotId)
        {
            var result = _simulationService.SimulateFault(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{robotId}")]
        public ActionResult<RobotCommandResultDto> RemoveRobot(int robotId)
        {
            var result = _simulationService.RemoveRobot(robotId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}


