using RobotShared;

namespace RobotServer.Models
{
    public class Robot
    {
        public int RobotId { get; set; }
        public string DisplayName { get; set; } = "";

        public RobotState State { get; set; } = RobotState.Idle;

        public PositionDto Position { get; set; } = new();
        public PositionDto? TargetPosition { get; set; }

        public string? TargetShelfId { get; set; }

        public bool CarryingLoad { get; set; }

        public int BatteryLevel { get; set; } = 100;
        public bool LowBatteryWarning { get; set; }

        public DiagnosticLevel DiagnosticLevel { get; set; } = DiagnosticLevel.Normal;
        public ComponentStatus MotorStatus { get; set; } = ComponentStatus.Normal;
        public ComponentStatus SensorStatus { get; set; } = ComponentStatus.Normal;

        public string? LastErrorMessage { get; set; }
    }
}
