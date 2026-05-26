using System;
using System.Collections.Generic;
using System.Linq;
using RobotServer.Models;
using RobotShared;

namespace RobotServer.Services
{
    public class RobotSimulationService
    {
        private readonly WarehouseService _warehouseService;

        private readonly List<Robot> _robots = new();
        private int _nextRobotNumber = 1;

        private readonly Random _random = new();

        public RobotSimulationService(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // ------------------------------------------------------------
        // GET ROBOTS
        // ------------------------------------------------------------

        public List<RobotDetailsDto> GetRobots()
        {
            return _robots.Select(ToRobotDetailsDto).ToList();
        }

        // ------------------------------------------------------------
        // GET STATISTICS
        // ------------------------------------------------------------

        public RobotStatisticsDto GetRobotStatistics()
        {
            return new RobotStatisticsDto
            {
                TotalRobots = _robots.Count,

                ErrorRobots = _robots.Count(robot =>
                    robot.State == RobotState.Error ||
                    robot.DiagnosticLevel == DiagnosticLevel.Error),

                WarningRobots = _robots.Count(robot =>
                    robot.DiagnosticLevel == DiagnosticLevel.Warning ||
                    robot.DiagnosticLevel == DiagnosticLevel.CriticalWarning),

                PausedRobots = _robots.Count(robot =>
                    robot.State == RobotState.Paused),

                AverageBatteryLevel = _robots.Count == 0
                    ? 0
                    : _robots.Average(robot => robot.BatteryLevel)
            };
        }

        // ------------------------------------------------------------
        // CREATE ROBOT
        // ------------------------------------------------------------

        public RobotCommandResultDto CreateRobot(CreateRobotRequestDto request)
        {
            var warehouse = _warehouseService.GetWarehouse();

            int robotId = _nextRobotNumber;
            string defaultDisplayName = $"ROBOT-{robotId:000}";

            var robot = new Robot
            {
                RobotId = robotId,

                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                    ? defaultDisplayName
                    : request.DisplayName,

                State = RobotState.Paused,

                Position = CopyPosition(warehouse.SpawnPosition),

                TargetPosition = null,
                TargetShelfId = null,

                CarryingLoad = false,

                BatteryLevel = 100,
                LowBatteryWarning = false,

                DiagnosticLevel = DiagnosticLevel.Normal,
                MotorStatus = ComponentStatus.Normal,
                SensorStatus = ComponentStatus.Normal,

                LastErrorMessage = null
            };

            _robots.Add(robot);
            _nextRobotNumber++;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot created successfully. Press resume to start autonomous operation.",
                RobotId = robot.RobotId
            };
        }

        // ------------------------------------------------------------
        // REMOVE ROBOT
        // ------------------------------------------------------------

        public RobotCommandResultDto RemoveRobot(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            _robots.Remove(robot);

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot removed successfully.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // RENAME ROBOT
        // ------------------------------------------------------------

        public RobotCommandResultDto RenameRobot(int robotId, RenameRobotRequestDto request)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (string.IsNullOrWhiteSpace(request.NewDisplayName))
            {
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Display name cannot be empty.",
                    RobotId = robotId
                };
            }

            robot.DisplayName = request.NewDisplayName;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot renamed successfully.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // PAUSE / RESUME
        // ------------------------------------------------------------

        public RobotCommandResultDto PauseRobot(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error)
            {
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Robot is in error state and cannot be paused.",
                    RobotId = robotId
                };
            }

            robot.State = RobotState.Paused;
            robot.TargetPosition = null;
            robot.TargetShelfId = null;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot paused successfully.",
                RobotId = robotId
            };
        }

        public RobotCommandResultDto ResumeRobot(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error)
            {
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Robot is in error state and must be fixed first.",
                    RobotId = robotId
                };
            }

            robot.State = RobotState.Idle;
            robot.TargetPosition = null;
            robot.TargetShelfId = null;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot resumed successfully. Autonomous operation continues.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // MOVE COMMANDS
        // ------------------------------------------------------------

        public RobotCommandResultDto MoveToCharger(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error)
            {
                return CannotMoveBecauseOfError(robotId);
            }

            var warehouse = _warehouseService.GetWarehouse();

            robot.TargetPosition = CopyPosition(warehouse.ChargerPosition);
            robot.TargetShelfId = null;
            robot.State = RobotState.MovingToCharger;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot target set to charger.",
                RobotId = robotId
            };
        }

        public RobotCommandResultDto MoveToService(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error)
            {
                return CannotMoveBecauseOfError(robotId);
            }

            var warehouse = _warehouseService.GetWarehouse();

            robot.TargetPosition = CopyPosition(warehouse.ServicePosition);
            robot.TargetShelfId = null;
            robot.State = RobotState.MovingToService;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Robot target set to service point.",
                RobotId = robotId
            };
        }

        public RobotCommandResultDto MoveToLocation(int robotId, MoveToLocationRequestDto request)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error)
            {
                return CannotMoveBecauseOfError(robotId);
            }

            var targetPosition = new PositionDto
            {
                X = request.X,
                Y = request.Y
            };

            if (!_warehouseService.IsInsideWarehouse(targetPosition))
            {
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Target position is outside the warehouse grid.",
                    RobotId = robotId
                };
            }

            robot.TargetPosition = targetPosition;
            robot.TargetShelfId = null;
            robot.State = RobotState.ManualMoving;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Manual target position set.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // DIAGNOSTICS
        // ------------------------------------------------------------

        public RobotCommandResultDto ClearWarning(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            if (robot.State == RobotState.Error || robot.DiagnosticLevel == DiagnosticLevel.Error)
            {
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Robot has an error. Use fix-error instead.",
                    RobotId = robotId
                };
            }

            robot.MotorStatus = ComponentStatus.Normal;
            robot.SensorStatus = ComponentStatus.Normal;
            robot.LastErrorMessage = null;

            UpdateDiagnosticLevel(robot);

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Warnings cleared successfully.",
                RobotId = robotId
            };
        }

        public RobotCommandResultDto FixError(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            robot.MotorStatus = ComponentStatus.Normal;
            robot.SensorStatus = ComponentStatus.Normal;
            robot.DiagnosticLevel = DiagnosticLevel.Normal;
            robot.LastErrorMessage = null;

            robot.TargetPosition = null;
            robot.TargetShelfId = null;
            robot.State = RobotState.Idle;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Error fixed successfully. Robot continues operation.",
                RobotId = robotId
            };
        }

        public SelfTestResultDto RunSelfTest(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return new SelfTestResultDto
                {
                    Success = false,
                    RobotExists = false,
                    RobotId = robotId,
                    Summary = "Self-test failed. Robot not found.",
                    Checks = new List<SelfTestItemDto>
            {
                new SelfTestItemDto
                {
                    Name = "Robot exists",
                    Passed = false,
                    Message = "Robot with the given ID does not exist."
                }
            }
                };
            }

            var checks = new List<SelfTestItemDto>();

            checks.Add(new SelfTestItemDto
            {
                Name = "Robot exists",
                Passed = true,
                Message = "Robot found successfully."
            });

            checks.Add(new SelfTestItemDto
            {
                Name = "Robot state",
                Passed = robot.State != RobotState.Error,
                Message = robot.State == RobotState.Error
                    ? "Robot is in Error state."
                    : $"Robot state is {robot.State}."
            });

            checks.Add(new SelfTestItemDto
            {
                Name = "Motor status",
                Passed = robot.MotorStatus != ComponentStatus.Error,
                Message = robot.MotorStatus == ComponentStatus.Error
                    ? "Motor has an error."
                    : $"Motor status is {robot.MotorStatus}."
            });

            checks.Add(new SelfTestItemDto
            {
                Name = "Sensor status",
                Passed = robot.SensorStatus != ComponentStatus.Error,
                Message = robot.SensorStatus == ComponentStatus.Error
                    ? "Sensor has an error."
                    : $"Sensor status is {robot.SensorStatus}."
            });

            checks.Add(new SelfTestItemDto
            {
                Name = "Battery level",
                Passed = robot.BatteryLevel > 20,
                Message = robot.BatteryLevel <= 20
                    ? $"Battery level is low: {robot.BatteryLevel}%."
                    : $"Battery level is acceptable: {robot.BatteryLevel}%."
            });

            bool allChecksPassed = checks.All(check => check.Passed);

            return new SelfTestResultDto
            {
                Success = allChecksPassed,
                RobotExists = true,
                RobotId = robot.RobotId,
                DisplayName = robot.DisplayName,
                Summary = allChecksPassed
                    ? "Self-test passed. Robot is ready for operation."
                    : "Self-test completed. One or more checks failed.",
                Checks = checks
            };
        }

        public RobotCommandResultDto SimulateFault(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            int faultType = _random.Next(0, 4);

            switch (faultType)
            {
                case 0:
                    robot.MotorStatus = ComponentStatus.Warning;
                    robot.LastErrorMessage = "Motor warning generated.";
                    break;

                case 1:
                    robot.SensorStatus = ComponentStatus.Warning;
                    robot.LastErrorMessage = "Sensor warning generated.";
                    break;

                case 2:
                    robot.MotorStatus = ComponentStatus.Error;
                    robot.State = RobotState.Error;
                    robot.LastErrorMessage = "Motor error generated.";
                    break;

                case 3:
                    robot.SensorStatus = ComponentStatus.Error;
                    robot.State = RobotState.Error;
                    robot.LastErrorMessage = "Sensor error generated.";
                    break;
            }

            UpdateDiagnosticLevel(robot);

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Fault simulated successfully.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // SELF TEST
        // ------------------------------------------------------------

        public RobotCommandResultDto SelfTest(int robotId)
        {
            var robot = FindRobot(robotId);

            if (robot == null)
            {
                return RobotNotFound(robotId);
            }

            bool hasCriticalFailure = robot.MotorStatus == ComponentStatus.Error || robot.SensorStatus == ComponentStatus.Error;

            if (hasCriticalFailure)
            {
                robot.LastErrorMessage = "Self-test failed: Hardware requires manual fix.";
                return new RobotCommandResultDto
                {
                    Success = false,
                    Message = "Self-test failed due to critical hardware error.",
                    RobotId = robotId
                };
            }

            if (robot.MotorStatus == ComponentStatus.Warning)
                robot.MotorStatus = ComponentStatus.Normal;
            
            if (robot.SensorStatus == ComponentStatus.Warning)
                robot.SensorStatus = ComponentStatus.Normal;

            robot.DiagnosticLevel = DiagnosticLevel.Normal;
            robot.LastErrorMessage = "Self-test completed. Systems nominal.";
            
            if (robot.State == RobotState.Error)
                robot.State = RobotState.Idle;

            return new RobotCommandResultDto
            {
                Success = true,
                Message = "Self-test passed successfully. All systems nominal.",
                RobotId = robotId
            };
        }

        // ------------------------------------------------------------
        // SIMULATION TICK
        // ------------------------------------------------------------

        public List<RobotDetailsDto> Tick()
        {
            foreach (var robot in _robots)
            {
                TickRobot(robot);
            }

            return GetRobots();
        }

        private void TickRobot(Robot robot)
        {
            if (robot.State == RobotState.Paused)
            {
                return;
            }

            if (robot.State == RobotState.Error)
            {
                return;
            }

            if (robot.State == RobotState.Charging)
            {
                ChargeRobot(robot);
                TryGenerateRandomFault(robot);
                UpdateDiagnosticLevel(robot);
                return;
            }

            // Manual movement has a special rule:
            // after reaching the manual target, the robot stays there until Resume or another MoveToLocation command.
            if (robot.State == RobotState.ManualMoving && robot.TargetPosition == null)
            {
                TryGenerateRandomFault(robot);
                UpdateDiagnosticLevel(robot);
                return;
            }

            ApplyLowBatteryRule(robot);

            if (robot.TargetPosition == null)
            {
                AssignNextAutonomousTarget(robot);
            }

            if (robot.TargetPosition != null)
            {
                bool moved = MoveOneStepTowardTarget(robot);

                if (moved)
                {
                    DecreaseBattery(robot);
                }

                if (HasReachedTarget(robot))
                {
                    HandleArrival(robot);
                }
            }

            TryGenerateRandomFault(robot);
            UpdateDiagnosticLevel(robot);
        }

        // ------------------------------------------------------------
        // RESET SIMULATION
        // ------------------------------------------------------------

        public SimulationResetResponseDto ResetSimulation()
        {
            _robots.Clear();
            _nextRobotNumber = 1;

            return new SimulationResetResponseDto
            {
                Success = true,
                Message = "Simulation reset successfully.",
                Warehouse = _warehouseService.GetWarehouse(),
                Robots = GetRobots()
            };
        }

        // ------------------------------------------------------------
        // AUTONOMOUS ROBOT BEHAVIOR
        // ------------------------------------------------------------

        private void AssignNextAutonomousTarget(Robot robot)
        {
            var warehouse = _warehouseService.GetWarehouse();

            if (robot.BatteryLevel <= 20 && !robot.CarryingLoad)
            {
                robot.LowBatteryWarning = true;
                robot.TargetPosition = CopyPosition(warehouse.ChargerPosition);
                robot.TargetShelfId = null;
                robot.State = RobotState.MovingToCharger;
                return;
            }

            if (robot.CarryingLoad)
            {
                robot.TargetPosition = CopyPosition(warehouse.DropoffPosition);
                robot.TargetShelfId = null;
                robot.State = RobotState.MovingToDropoff;
                return;
            }

            if (warehouse.Shelves.Count > 0)
            {
                var shelf = warehouse.Shelves[_random.Next(warehouse.Shelves.Count)];

                robot.TargetPosition = CopyPosition(shelf.Position);
                robot.TargetShelfId = shelf.ShelfId;
                robot.State = RobotState.MovingToShelf;
            }
        }

        private void ApplyLowBatteryRule(Robot robot)
        {
            if (robot.BatteryLevel > 20)
            {
                return;
            }

            robot.LowBatteryWarning = true;

            if (robot.CarryingLoad)
            {
                return;
            }

            if (robot.State == RobotState.MovingToCharger ||
                robot.State == RobotState.Charging ||
                robot.State == RobotState.MovingToService ||
                robot.State == RobotState.ManualMoving)
            {
                return;
            }

            var warehouse = _warehouseService.GetWarehouse();

            robot.TargetPosition = CopyPosition(warehouse.ChargerPosition);
            robot.TargetShelfId = null;
            robot.State = RobotState.MovingToCharger;
        }

        private void HandleArrival(Robot robot)
        {
            var warehouse = _warehouseService.GetWarehouse();

            switch (robot.State)
            {
                case RobotState.MovingToShelf:
                    robot.CarryingLoad = true;
                    robot.TargetPosition = CopyPosition(warehouse.DropoffPosition);
                    robot.TargetShelfId = null;
                    robot.State = RobotState.MovingToDropoff;
                    break;

                case RobotState.MovingToDropoff:
                    robot.CarryingLoad = false;
                    robot.TargetShelfId = null;

                    if (robot.BatteryLevel <= 20)
                    {
                        robot.LowBatteryWarning = true;
                        robot.TargetPosition = CopyPosition(warehouse.ChargerPosition);
                        robot.State = RobotState.MovingToCharger;
                    }
                    else
                    {
                        robot.TargetPosition = null;
                        robot.State = RobotState.Idle;
                    }

                    break;

                case RobotState.MovingToCharger:
                    robot.TargetPosition = null;
                    robot.TargetShelfId = null;
                    robot.State = RobotState.Charging;
                    break;

                case RobotState.MovingToService:
                    robot.TargetPosition = null;
                    robot.TargetShelfId = null;

                    robot.MotorStatus = ComponentStatus.Normal;
                    robot.SensorStatus = ComponentStatus.Normal;
                    robot.DiagnosticLevel = DiagnosticLevel.Normal;
                    robot.LastErrorMessage = null;

                    robot.State = RobotState.Idle;
                    break;

                case RobotState.ManualMoving:
                    robot.TargetPosition = null;
                    robot.TargetShelfId = null;

                    // Important:
                    // stay in ManualMoving so the robot does not automatically continue.
                    // ResumeRobot() will set it back to Idle.
                    robot.State = RobotState.ManualMoving;
                    break;
            }
        }

        // ------------------------------------------------------------
        // MOVEMENT / BATTERY
        // ------------------------------------------------------------

        private bool MoveOneStepTowardTarget(Robot robot)
        {
            if (robot.TargetPosition == null)
            {
                return false;
            }

            if (HasReachedTarget(robot))
            {
                return false;
            }

            if (robot.Position.X < robot.TargetPosition.X)
            {
                robot.Position.X++;
                return true;
            }

            if (robot.Position.X > robot.TargetPosition.X)
            {
                robot.Position.X--;
                return true;
            }

            if (robot.Position.Y < robot.TargetPosition.Y)
            {
                robot.Position.Y++;
                return true;
            }

            if (robot.Position.Y > robot.TargetPosition.Y)
            {
                robot.Position.Y--;
                return true;
            }

            return false;
        }

        private bool HasReachedTarget(Robot robot)
        {
            if (robot.TargetPosition == null)
            {
                return false;
            }

            return robot.Position.X == robot.TargetPosition.X &&
                   robot.Position.Y == robot.TargetPosition.Y;
        }

        private void DecreaseBattery(Robot robot)
        {
            if (robot.BatteryLevel > 0)
            {
                robot.BatteryLevel--;
            }

            if (robot.BatteryLevel <= 20)
            {
                robot.LowBatteryWarning = true;
            }
        }

        private void ChargeRobot(Robot robot)
        {
            if (robot.BatteryLevel < 100)
            {
                robot.BatteryLevel += 5;
            }

            if (robot.BatteryLevel >= 100)
            {
                robot.BatteryLevel = 100;
                robot.LowBatteryWarning = false;
                robot.TargetPosition = null;
                robot.TargetShelfId = null;
                robot.State = RobotState.Idle;
            }
        }

        // ------------------------------------------------------------
        // RANDOM FAULT LOGIC
        // ------------------------------------------------------------

        private void TryGenerateRandomFault(Robot robot)
        {
            if (robot.State == RobotState.Error)
            {
                return;
            }

            // Very low chance per tick.
            int chance = _random.Next(0, 1000); // <======================================= random fault chance ===============================

            if (chance == 1)
            {
                robot.MotorStatus = ComponentStatus.Warning;
                robot.LastErrorMessage = "Random motor warning.";
            }
            else if (chance == 2)
            {
                robot.SensorStatus = ComponentStatus.Warning;
                robot.LastErrorMessage = "Random sensor warning.";
            }
            else if (chance == 3)
            {
                robot.MotorStatus = ComponentStatus.Error;
                robot.LastErrorMessage = "Random motor error.";
            }
            else if (chance == 4)
            {
                robot.SensorStatus = ComponentStatus.Error;
                robot.LastErrorMessage = "Random sensor error.";
            }
        }

        // ------------------------------------------------------------
        // DIAGNOSTIC LEVEL
        // ------------------------------------------------------------

        private void UpdateDiagnosticLevel(Robot robot)
        {
            if (robot.MotorStatus == ComponentStatus.Error ||
                robot.SensorStatus == ComponentStatus.Error)
            {
                robot.DiagnosticLevel = DiagnosticLevel.Error;
                robot.State = RobotState.Error;
                return;
            }

            if (robot.MotorStatus == ComponentStatus.Warning &&
                robot.SensorStatus == ComponentStatus.Warning)
            {
                robot.DiagnosticLevel = DiagnosticLevel.CriticalWarning;
                return;
            }

            if (robot.MotorStatus == ComponentStatus.Warning ||
                robot.SensorStatus == ComponentStatus.Warning)
            {
                robot.DiagnosticLevel = DiagnosticLevel.Warning;
                return;
            }

            robot.DiagnosticLevel = DiagnosticLevel.Normal;
        }

        // ------------------------------------------------------------
        // DTO CONVERSION
        // ------------------------------------------------------------

        private RobotDetailsDto ToRobotDetailsDto(Robot robot)
        {
            return new RobotDetailsDto
            {
                RobotId = robot.RobotId,
                DisplayName = robot.DisplayName,

                State = robot.State,

                Position = CopyPosition(robot.Position),

                TargetPosition = robot.TargetPosition == null
                    ? null
                    : CopyPosition(robot.TargetPosition),

                TargetShelfId = robot.TargetShelfId,

                CarryingLoad = robot.CarryingLoad,
                BatteryLevel = robot.BatteryLevel,
                LowBatteryWarning = robot.LowBatteryWarning,

                DiagnosticLevel = robot.DiagnosticLevel,
                MotorStatus = robot.MotorStatus,
                SensorStatus = robot.SensorStatus,

                LastErrorMessage = robot.LastErrorMessage
            };
        }

        // ------------------------------------------------------------
        // HELPER METHODS
        // ------------------------------------------------------------

        private Robot? FindRobot(int robotId)
        {
            return _robots.FirstOrDefault(r => r.RobotId == robotId);
        }

        private PositionDto CopyPosition(PositionDto position)
        {
            return new PositionDto
            {
                X = position.X,
                Y = position.Y
            };
        }

        private RobotCommandResultDto RobotNotFound(int robotId)
        {
            return new RobotCommandResultDto
            {
                Success = false,
                Message = "Robot not found.",
                RobotId = robotId
            };
        }

        private RobotCommandResultDto CannotMoveBecauseOfError(int robotId)
        {
            return new RobotCommandResultDto
            {
                Success = false,
                Message = "Robot is in error state and cannot move. Fix the error first.",
                RobotId = robotId
            };
        }
    }
}