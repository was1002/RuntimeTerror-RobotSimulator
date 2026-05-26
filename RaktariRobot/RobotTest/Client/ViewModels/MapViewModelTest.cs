using Xunit;
using RuntimeTerror.Client.ViewModels;
using RobotShared;
using Microsoft.Maui.Graphics;

namespace RobotTest.Client.ViewModels
{
    public class MapViewModelTest
    {
        [Fact]
        public void MapViewModel_DefaultState_IsCorrect()
        {
            var vm = new MapViewModel();

            Assert.Empty(vm.MapCells);
            Assert.Empty(vm.MapRobots);
            Assert.Equal(40, vm.CellSize);
        }

        [Fact]
        public void GenerateMap_ShouldPopulateMapCellsCorrectly()
        {
            // Arrange
            var vm = new MapViewModel();
            var warehouse = new WarehouseDto
            {
                Width = 5,
                Height = 5,
                SpawnPosition = new PositionDto { X = 0, Y = 0 },
                DropoffPosition = new PositionDto { X = 1, Y = 1 },
                ChargerPosition = new PositionDto { X = 2, Y = 2 },
                ServicePosition = new PositionDto { X = 3, Y = 3 },
                Shelves = new System.Collections.Generic.List<ShelfDto>
                {
                    new ShelfDto { Position = new PositionDto { X = 4, Y = 4 } }
                }
            };

            // Act
            vm.GenerateMap(warehouse);

            // Assert
            Assert.Equal(25, vm.MapCells.Count);

            // Check specific icons based on the GenerateMap logic
            // x=0, y=0 -> Spawn -> "R"
            var spawnCell = vm.MapCells[0]; // 0*5 + 0
            Assert.Equal("R", spawnCell.Icon);
            Assert.Equal(Colors.LightBlue, spawnCell.BgColor);

            // x=1, y=1 -> Dropoff -> "D"
            var dropoffCell = vm.MapCells[6]; // 1*5 + 1
            Assert.Equal("D", dropoffCell.Icon);
            Assert.Equal(Colors.Orange, dropoffCell.BgColor);

            // x=2, y=2 -> Charger -> "C"
            var chargerCell = vm.MapCells[12]; // 2*5 + 2
            Assert.Equal("C", chargerCell.Icon);
            Assert.Equal(Colors.Yellow, chargerCell.BgColor);

            // x=4, y=4 -> Shelf -> "S"
            var shelfCell = vm.MapCells[24]; // 4*5 + 4
            Assert.Equal("S", shelfCell.Icon);
            Assert.Equal(Colors.SaddleBrown, shelfCell.BgColor);
        }
    }
}