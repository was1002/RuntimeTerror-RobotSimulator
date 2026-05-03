namespace RobotShared
{
    public enum RobotState
    {
        Idle,
        MovingToShelf,
        MovingToDropoff,
        MovingToCharger,
        MovingToService,
        ManualMoving,
        Charging,
        Paused,
        Error
    }

    public enum DiagnosticLevel
    {
        Normal,
        Warning,
        CriticalWarning,
        Error
    }

    public enum ComponentStatus
    {
        Normal,
        Warning,
        Error
    }
}
