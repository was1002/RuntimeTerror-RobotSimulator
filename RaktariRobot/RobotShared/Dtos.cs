namespace RobotShared
{
    public class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ShelfDto
    {
        public string ShelfId { get; set; } = "";
        public PositionDto Position { get; set; } = new();
    }

    public class WarehouseDto
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public PositionDto SpawnPosition { get; set; } = new();
        public PositionDto DropoffPosition { get; set; } = new();
        public PositionDto ChargerPosition { get; set; } = new();
        public PositionDto ServicePosition { get; set; } = new();

        public List<ShelfDto> Shelves { get; set; } = new();
    }

    public class RobotDetailsDto
    {
        public int RobotId { get; set; }
        public string DisplayName { get; set; } = "";

        public RobotState State { get; set; }

        public PositionDto Position { get; set; } = new();
        public PositionDto? TargetPosition { get; set; }

        public string? TargetShelfId { get; set; }

        public bool CarryingLoad { get; set; }
        public int BatteryLevel { get; set; }
        public bool LowBatteryWarning { get; set; }

        public DiagnosticLevel DiagnosticLevel { get; set; }
        public ComponentStatus MotorStatus { get; set; }
        public ComponentStatus SensorStatus { get; set; }

        public string? LastErrorMessage { get; set; }
    }

    public class CreateRobotRequestDto
    {
        public string DisplayName { get; set; } = "";
    }

    public class RenameRobotRequestDto
    {
        public string NewDisplayName { get; set; } = "";
    }

    public class MoveToLocationRequestDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class RobotCommandResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int? RobotId { get; set; }
    }

    public class RobotStatisticsDto
    {
        public int TotalRobots { get; set; }

        public int ErrorRobots { get; set; }
        public int WarningRobots { get; set; }
        public int PausedRobots { get; set; }

        public double AverageBatteryLevel { get; set; }
    }

    public class SelfTestResultDto
    {
        public bool Success { get; set; }
        public bool RobotExists { get; set; }

        public int? RobotId { get; set; }
        public string DisplayName { get; set; } = "";

        public string Summary { get; set; } = "";

        public List<SelfTestItemDto> Checks { get; set; } = new();
    }

    public class SelfTestItemDto
    {
        public string Name { get; set; } = "";
        public bool Passed { get; set; }
        public string Message { get; set; } = "";
    }

    public class SimulationResetResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public WarehouseDto Warehouse { get; set; } = new();
        public List<RobotDetailsDto> Robots { get; set; } = new();
    }
}
