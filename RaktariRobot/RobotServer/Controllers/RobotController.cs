using Microsoft.AspNetCore.Mvc;
using RobotShared;
using System.Collections.Generic;
using System.Linq;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/robots")] // Új útvonal: api/robots
    public class RobotsController : ControllerBase
    {
        // Késõbb ezt érdemes egy külön "RobotSimulationService"-be kiszervezni!
        private static List<RobotDetailsDto> _robots = new List<RobotDetailsDto>();
        private static int _nextRobotId = 1;

        // --- GET Endpoints ---
        
        [HttpGet]
        public ActionResult<List<RobotDetailsDto>> GetAllRobots()
        {
            return Ok(_robots);
        }

        // --- Robot Command Endpoints ---

        [HttpPost]
        public ActionResult<RobotCommandResultDto> CreateRobot([FromBody] CreateRobotRequestDto request)
        {
            var newRobot = new RobotDetailsDto
            {
                RobotId = _nextRobotId++,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? $"ROBOT-{_nextRobotId - 1:D3}" : request.DisplayName,
                State = RobotState.Idle, // Alapértelmezett állapot, amíg a Resume gombot meg nem nyomják
                Position = new PositionDto { X = 0, Y = 2 }, // Spawn pont
                BatteryLevel = 100,
                DiagnosticLevel = DiagnosticLevel.Normal,
                MotorStatus = ComponentStatus.Normal,
                SensorStatus = ComponentStatus.Normal
            };

            _robots.Add(newRobot);

            return Ok(new RobotCommandResultDto { Success = true, Message = "Robot created.", RobotId = newRobot.RobotId });
        }

        [HttpPost("{robotId}/rename")]
        public ActionResult<RobotCommandResultDto> RenameRobot(int robotId, [FromBody] RenameRobotRequestDto request)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.DisplayName = request.NewDisplayName;
            return Ok(new RobotCommandResultDto { Success = true, Message = "Robot renamed." });
        }

        [HttpPost("{robotId}/resume")]
        public ActionResult<RobotCommandResultDto> ResumeRobot(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            if (robot.State == RobotState.Paused || robot.State == RobotState.Idle)
            {
                robot.State = RobotState.MovingToShelf; // Vagy egy default logikai állapot
            }
            return Ok(new RobotCommandResultDto { Success = true, Message = "Robot resumed." });
        }

        [HttpPost("{robotId}/pause")]
        public ActionResult<RobotCommandResultDto> PauseRobot(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.State = RobotState.Paused;
            return Ok(new RobotCommandResultDto { Success = true, Message = "Robot paused." });
        }

        [HttpPost("{robotId}/move-to-charger")]
        public ActionResult<RobotCommandResultDto> MoveToCharger(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.State = RobotState.MovingToCharger;
            robot.TargetPosition = new PositionDto { X = 9, Y = 3 }; // Charger pozíció a doksi szerint
            return Ok(new RobotCommandResultDto { Success = true, Message = "Moving to charger." });
        }

        [HttpPost("{robotId}/move-to-service")]
        public ActionResult<RobotCommandResultDto> MoveToService(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.State = RobotState.MovingToService;
            robot.TargetPosition = new PositionDto { X = 2, Y = 0 }; // Service (W) pozíció
            return Ok(new RobotCommandResultDto { Success = true, Message = "Moving to service." });
        }

        [HttpPost("{robotId}/move-to-location")]
        public ActionResult<RobotCommandResultDto> MoveToLocation(int robotId, [FromBody] MoveToLocationRequestDto request)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.State = RobotState.ManualMoving;
            robot.TargetPosition = new PositionDto { X = request.X, Y = request.Y };
            return Ok(new RobotCommandResultDto { Success = true, Message = $"Moving to ({request.X}, {request.Y})." });
        }

        [HttpPost("{robotId}/clear-warning")]
        public ActionResult<RobotCommandResultDto> ClearWarning(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            if (robot.DiagnosticLevel == DiagnosticLevel.Warning || robot.DiagnosticLevel == DiagnosticLevel.CriticalWarning)
            {
                robot.DiagnosticLevel = DiagnosticLevel.Normal;
                robot.MotorStatus = ComponentStatus.Normal;
                robot.SensorStatus = ComponentStatus.Normal;
            }
            return Ok(new RobotCommandResultDto { Success = true, Message = "Warnings cleared." });
        }

        [HttpPost("{robotId}/fix-error")]
        public ActionResult<RobotCommandResultDto> FixError(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.State = RobotState.Idle; // Miután megjavították, újraindulhat
            robot.DiagnosticLevel = DiagnosticLevel.Normal;
            robot.MotorStatus = ComponentStatus.Normal;
            robot.SensorStatus = ComponentStatus.Normal;
            robot.LastErrorMessage = null;
            
            return Ok(new RobotCommandResultDto { Success = true, Message = "Error fixed." });
        }

        [HttpPost("{robotId}/simulate-fault")]
        public ActionResult<RobotCommandResultDto> SimulateFault(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            robot.DiagnosticLevel = DiagnosticLevel.Warning;
            robot.SensorStatus = ComponentStatus.Warning; // Példa egy hibára

            return Ok(new RobotCommandResultDto { Success = true, Message = "Fault simulated." });
        }

        [HttpDelete("{robotId}")]
        public ActionResult<RobotCommandResultDto> RemoveRobot(int robotId)
        {
            var robot = _robots.FirstOrDefault(r => r.RobotId == robotId);
            if (robot == null) return NotFound(new RobotCommandResultDto { Success = false, Message = "Robot not found." });

            _robots.Remove(robot);
            return Ok(new RobotCommandResultDto { Success = true, Message = "Robot removed." });
        }
    }
}


