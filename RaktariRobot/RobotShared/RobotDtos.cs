using System;
using System.Collections.Generic;

namespace RobotShared
{
    // ==========================================
    // Enums
    // ==========================================

    public enum RobotState
    {
        Ready,
        Moving,
        Loading,
        Unloading,
        Charging,
        Paused,
        EmergencyStop,
        Error,
        Disconnected
    }

    public enum DiagnosticLevel
    {
        Normal,
        Warning,
        Error
    }

    public enum ComponentStatus
    {
        Normal,
        Warning,
        Error
    }

    // ==========================================
    // Basic DTOs
    // ==========================================

    public class PositionDto
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class RobotSummaryDto
    {
        public string RobotId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public RobotState State { get; set; }
        public int BatteryLevel { get; set; }
        public DiagnosticLevel DiagnosticLevel { get; set; }
    }

    public class RobotDetailsDto
    {
        public string RobotId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastSeenAt { get; set; }
        public RobotState State { get; set; }
        public string? AssignedTask { get; set; }
        public PositionDto Position { get; set; } = new();
        public PositionDto TargetPosition { get; set; } = new();
        public bool CarryingLoad { get; set; }
        public int BatteryLevel { get; set; }
        public int EstimatedRemainingMinutes { get; set; }
        public bool LowBatteryWarning { get; set; }
        public DiagnosticLevel DiagnosticLevel { get; set; }
        public ComponentStatus MotorStatus { get; set; }
        public ComponentStatus SensorStatus { get; set; }
        public string? LastErrorCode { get; set; }
        public string? LastErrorMessage { get; set; }
        public int UptimeSeconds { get; set; }
        public bool EmergencyStopActive { get; set; }
    }

    // ==========================================
    // GET Response DTOs
    // ==========================================

    public class RobotStateDto
    {
        public string RobotId { get; set; } = string.Empty;
        public RobotState State { get; set; }
        public DiagnosticLevel DiagnosticLevel { get; set; }
        public bool EmergencyStopActive { get; set; }
    }

    public class RobotPositionDto
    {
        public string RobotId { get; set; } = string.Empty;
        public PositionDto Position { get; set; } = new();
        public PositionDto TargetPosition { get; set; } = new();
        public bool CarryingLoad { get; set; }
    }

    public class RobotBatteryDto
    {
        public string RobotId { get; set; } = string.Empty;
        public int BatteryLevel { get; set; }
        public int EstimatedRemainingMinutes { get; set; }
        public bool LowBatteryWarning { get; set; }
        public RobotState State { get; set; }
    }

    public class RobotErrorDto
    {
        public string RobotId { get; set; } = string.Empty;
        public string? LastErrorCode { get; set; }
        public DiagnosticLevel DiagnosticLevel { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RobotHistoryEventDto
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class RobotHistoryDto
    {
        public string RobotId { get; set; } = string.Empty;
        public List<RobotHistoryEventDto> Events { get; set; } = new();
    }

    // ==========================================
    // POST Request DTOs
    // ==========================================

    public class MoveRobotRequestDto
    {
        public PositionDto TargetPosition { get; set; } = new();
    }

    public class PauseRobotRequestDto
    {
    }

    public class ChargeRobotRequestDto
    {
    }

    public class EmergencyStopRequestDto
    {
    }

    public class ResetErrorRequestDto
    {
    }

    public class SimulateErrorRequestDto
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class SimulateLowBatteryRequestDto
    {
        public int BatteryLevel { get; set; }
    }

    public class SimulateDisconnectRequestDto
    {
        public int? DurationSeconds { get; set; }
    }

    // ==========================================
    // POST Response DTOs
    // ==========================================

    public class RobotCommandResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RobotId { get; set; } = string.Empty;
        public RobotState State { get; set; }
        public DiagnosticLevel DiagnosticLevel { get; set; }
    }
}

