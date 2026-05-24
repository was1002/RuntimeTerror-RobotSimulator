using RobotServer.Services;
using RobotShared;
using Xunit;

namespace RobotTest.Server
{
    public class RobotServerServiceTests
    {
        private static WarehouseService CreateWarehouseService()
        {
            return new WarehouseService();
        }

        private static RobotSimulationService CreateSimulationService()
        {
            var warehouseService = CreateWarehouseService();
            return new RobotSimulationService(warehouseService);
        }

        // ------------------------------------------------------------
        // WAREHOUSE SERVICE TESTS
        // ------------------------------------------------------------

        [Fact]
        public void GetWarehouse_ReturnsExpectedFixedWarehouseLayout()
        {
            // Arrange
            var warehouseService = CreateWarehouseService();

            // Act
            var warehouse = warehouseService.GetWarehouse();

            // Assert
            Assert.Equal(10, warehouse.Width);
            Assert.Equal(7, warehouse.Height);

            Assert.Equal(6, warehouse.SpawnPosition.X);
            Assert.Equal(2, warehouse.SpawnPosition.Y);

            Assert.Equal(9, warehouse.ChargerPosition.X);
            Assert.Equal(3, warehouse.ChargerPosition.Y);

            Assert.Equal(8, warehouse.DropoffPosition.X);
            Assert.Equal(6, warehouse.DropoffPosition.Y);

            Assert.Equal(1, warehouse.ServicePosition.X);
            Assert.Equal(0, warehouse.ServicePosition.Y);

            Assert.Equal(5, warehouse.Shelves.Count);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(9, 6)]
        [InlineData(6, 2)]
        public void IsInsideWarehouse_WithValidCoordinates_ReturnsTrue(int x, int y)
        {
            // Arrange
            var warehouseService = CreateWarehouseService();

            var position = new PositionDto
            {
                X = x,
                Y = y
            };

            // Act
            bool result = warehouseService.IsInsideWarehouse(position);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(10, 0)]
        [InlineData(0, 7)]
        public void IsInsideWarehouse_WithInvalidCoordinates_ReturnsFalse(int x, int y)
        {
            // Arrange
            var warehouseService = CreateWarehouseService();

            var position = new PositionDto
            {
                X = x,
                Y = y
            };

            // Act
            bool result = warehouseService.IsInsideWarehouse(position);

            // Assert
            Assert.False(result);
        }

        // ------------------------------------------------------------
        // ROBOT CREATION / GET ROBOTS TESTS
        // ------------------------------------------------------------

        [Fact]
        public void GetRobots_WhenNoRobotsCreated_ReturnsEmptyList()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            // Act
            var robots = simulationService.GetRobots();

            // Assert
            Assert.Empty(robots);
        }

        [Fact]
        public void CreateRobot_WithEmptyDisplayName_CreatesPausedRobotAtSpawn()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            var request = new CreateRobotRequestDto
            {
                DisplayName = ""
            };

            // Act
            var result = simulationService.CreateRobot(request);
            var robots = simulationService.GetRobots();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.RobotId);

            Assert.Single(robots);

            var robot = robots[0];

            Assert.Equal(1, robot.RobotId);
            Assert.Equal("ROBOT-001", robot.DisplayName);
            Assert.Equal(RobotState.Paused, robot.State);

            Assert.Equal(6, robot.Position.X);
            Assert.Equal(2, robot.Position.Y);

            Assert.False(robot.CarryingLoad);
            Assert.Equal(100, robot.BatteryLevel);
            Assert.False(robot.LowBatteryWarning);

            Assert.Equal(DiagnosticLevel.Normal, robot.DiagnosticLevel);
            Assert.Equal(ComponentStatus.Normal, robot.MotorStatus);
            Assert.Equal(ComponentStatus.Normal, robot.SensorStatus);
            Assert.Null(robot.LastErrorMessage);
        }

        [Fact]
        public void CreateRobot_WhenCalledTwice_AssignsIncreasingRobotIds()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            // Act
            var firstResult = simulationService.CreateRobot(new CreateRobotRequestDto());
            var secondResult = simulationService.CreateRobot(new CreateRobotRequestDto());

            var robots = simulationService.GetRobots();

            // Assert
            Assert.True(firstResult.Success);
            Assert.True(secondResult.Success);

            Assert.Equal(1, firstResult.RobotId);
            Assert.Equal(2, secondResult.RobotId);

            Assert.Equal(2, robots.Count);
            Assert.Equal("ROBOT-001", robots[0].DisplayName);
            Assert.Equal("ROBOT-002", robots[1].DisplayName);
        }

        [Fact]
        public void CreateRobot_WithCustomDisplayName_UsesCustomDisplayName()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            var request = new CreateRobotRequestDto
            {
                DisplayName = "Test Robot"
            };

            // Act
            var result = simulationService.CreateRobot(request);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Test Robot", robot.DisplayName);
        }

        // ------------------------------------------------------------
        // RENAME TESTS
        // ------------------------------------------------------------

        [Fact]
        public void RenameRobot_WithValidName_ChangesDisplayName()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var request = new RenameRobotRequestDto
            {
                NewDisplayName = "Renamed Robot"
            };

            // Act
            var result = simulationService.RenameRobot(1, request);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Renamed Robot", robot.DisplayName);
            Assert.Equal(1, robot.RobotId);
        }

        [Fact]
        public void RenameRobot_WithEmptyName_ReturnsFailure()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var request = new RenameRobotRequestDto
            {
                NewDisplayName = ""
            };

            // Act
            var result = simulationService.RenameRobot(1, request);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void RenameRobot_WithMissingRobot_ReturnsFailure()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            var request = new RenameRobotRequestDto
            {
                NewDisplayName = "New Name"
            };

            // Act
            var result = simulationService.RenameRobot(999, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(999, result.RobotId);
        }

        // ------------------------------------------------------------
        // PAUSE / RESUME TESTS
        // ------------------------------------------------------------

        [Fact]
        public void ResumeRobot_ForPausedRobot_ChangesStateToIdle()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var result = simulationService.ResumeRobot(1);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(RobotState.Idle, robot.State);
        }

        [Fact]
        public void PauseRobot_ForExistingRobot_ChangesStateToPaused()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());
            simulationService.ResumeRobot(1);

            // Act
            var result = simulationService.PauseRobot(1);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(RobotState.Paused, robot.State);
        }

        [Fact]
        public void Tick_WhenRobotIsPaused_DoesNotMoveRobot()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var robotBeforeTick = simulationService.GetRobots().Single();

            // Act
            simulationService.Tick();

            var robotAfterTick = simulationService.GetRobots().Single();

            // Assert
            Assert.Equal(RobotState.Paused, robotAfterTick.State);

            Assert.Equal(robotBeforeTick.Position.X, robotAfterTick.Position.X);
            Assert.Equal(robotBeforeTick.Position.Y, robotAfterTick.Position.Y);

            Assert.Equal(100, robotAfterTick.BatteryLevel);
        }

        // ------------------------------------------------------------
        // MOVE TO LOCATION TESTS
        // ------------------------------------------------------------

        [Fact]
        public void MoveToLocation_WithValidCoordinate_SetsManualTarget()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var request = new MoveToLocationRequestDto
            {
                X = 5,
                Y = 2
            };

            // Act
            var result = simulationService.MoveToLocation(1, request);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);

            Assert.Equal(RobotState.ManualMoving, robot.State);
            Assert.NotNull(robot.TargetPosition);
            Assert.Equal(5, robot.TargetPosition!.X);
            Assert.Equal(2, robot.TargetPosition.Y);
            Assert.Null(robot.TargetShelfId);

            // Command should not move the robot immediately.
            Assert.Equal(6, robot.Position.X);
            Assert.Equal(2, robot.Position.Y);
        }

        [Fact]
        public void MoveToLocation_WithInvalidCoordinate_ReturnsFailure()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var request = new MoveToLocationRequestDto
            {
                X = 10,
                Y = 0
            };

            // Act
            var result = simulationService.MoveToLocation(1, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(1, result.RobotId);
        }

        [Fact]
        public void Tick_AfterMoveToLocation_MovesRobotOneStepAndDecreasesBattery()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            var request = new MoveToLocationRequestDto
            {
                X = 5,
                Y = 2
            };

            simulationService.MoveToLocation(1, request);

            // Act
            var robotsAfterTick = simulationService.Tick();
            var robot = robotsAfterTick.Single();

            // Assert
            Assert.Equal(5, robot.Position.X);
            Assert.Equal(2, robot.Position.Y);
            Assert.Equal(99, robot.BatteryLevel);
        }

        // ------------------------------------------------------------
        // MOVE TO CHARGER TESTS
        // ------------------------------------------------------------

        [Fact]
        public void MoveToCharger_SetsChargerAsTargetButDoesNotMoveImmediately()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var result = simulationService.MoveToCharger(1);
            var robot = simulationService.GetRobots().Single();

            // Assert
            Assert.True(result.Success);

            Assert.Equal(RobotState.MovingToCharger, robot.State);

            Assert.NotNull(robot.TargetPosition);
            Assert.Equal(9, robot.TargetPosition!.X);
            Assert.Equal(3, robot.TargetPosition.Y);

            // Command should only set intent. Movement happens on tick.
            Assert.Equal(6, robot.Position.X);
            Assert.Equal(2, robot.Position.Y);
        }

        [Fact]
        public void Tick_AfterMoveToCharger_MovesRobotOneStep()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());
            simulationService.MoveToCharger(1);

            // Act
            var robotsAfterTick = simulationService.Tick();
            var robot = robotsAfterTick.Single();

            // Assert
            Assert.Equal(7, robot.Position.X);
            Assert.Equal(2, robot.Position.Y);
            Assert.Equal(99, robot.BatteryLevel);
        }

        // ------------------------------------------------------------
        // STATISTICS TESTS
        // ------------------------------------------------------------

        [Fact]
        public void GetRobotStatistics_WithNoRobots_ReturnsZeroValues()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            // Act
            var statistics = simulationService.GetRobotStatistics();

            // Assert
            Assert.Equal(0, statistics.TotalRobots);
            Assert.Equal(0, statistics.ErrorRobots);
            Assert.Equal(0, statistics.WarningRobots);
            Assert.Equal(0, statistics.PausedRobots);
            Assert.Equal(0, statistics.AverageBatteryLevel);
        }

        [Fact]
        public void GetRobotStatistics_WithTwoPausedRobots_ReturnsCorrectTotals()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());
            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var statistics = simulationService.GetRobotStatistics();

            // Assert
            Assert.Equal(2, statistics.TotalRobots);
            Assert.Equal(0, statistics.ErrorRobots);
            Assert.Equal(0, statistics.WarningRobots);
            Assert.Equal(2, statistics.PausedRobots);
            Assert.Equal(100, statistics.AverageBatteryLevel);
        }

        // ------------------------------------------------------------
        // SELF-TEST TESTS
        // ------------------------------------------------------------

        [Fact]
        public void RunSelfTest_ForExistingHealthyRobot_ReturnsSuccessfulResult()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var result = simulationService.RunSelfTest(1);

            // Assert
            Assert.True(result.RobotExists);
            Assert.True(result.Success);
            Assert.Equal(1, result.RobotId);
            Assert.Equal("ROBOT-001", result.DisplayName);
            Assert.NotEmpty(result.Checks);
            Assert.All(result.Checks, check => Assert.True(check.Passed));
        }

        [Fact]
        public void RunSelfTest_ForMissingRobot_ReturnsFailure()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            // Act
            var result = simulationService.RunSelfTest(999);

            // Assert
            Assert.False(result.Success);
            Assert.False(result.RobotExists);
            Assert.Equal(999, result.RobotId);
            Assert.NotEmpty(result.Checks);
            Assert.Contains(result.Checks, check => check.Name == "Robot exists" && !check.Passed);
        }

        // ------------------------------------------------------------
        // RESET TESTS
        // ------------------------------------------------------------

        [Fact]
        public void ResetSimulation_AfterCreatingRobots_ClearsRobotList()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());
            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var result = simulationService.ResetSimulation();
            var robots = simulationService.GetRobots();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Robots);
            Assert.Empty(robots);
            Assert.NotNull(result.Warehouse);
        }

        [Fact]
        public void ResetSimulation_AfterCreatingRobots_ResetsRobotIdCounter()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());
            simulationService.CreateRobot(new CreateRobotRequestDto());

            simulationService.ResetSimulation();

            // Act
            var result = simulationService.CreateRobot(new CreateRobotRequestDto());

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.RobotId);
        }

        // ------------------------------------------------------------
        // REMOVE ROBOT TESTS
        // ------------------------------------------------------------

        [Fact]
        public void RemoveRobot_ForExistingRobot_RemovesRobot()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            simulationService.CreateRobot(new CreateRobotRequestDto());

            // Act
            var result = simulationService.RemoveRobot(1);
            var robots = simulationService.GetRobots();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(robots);
        }

        [Fact]
        public void RemoveRobot_ForMissingRobot_ReturnsFailure()
        {
            // Arrange
            var simulationService = CreateSimulationService();

            // Act
            var result = simulationService.RemoveRobot(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(999, result.RobotId);
        }
    }
}