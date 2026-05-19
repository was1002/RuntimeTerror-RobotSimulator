using RobotShared;

namespace RobotServer.Services
{
    public class WarehouseService
    {
        public WarehouseDto GetWarehouse()
        {
            return new WarehouseDto
            {
                Width = 10,
                Height = 7,

                SpawnPosition = new PositionDto { X = 6, Y = 2 },
                ChargerPosition = new PositionDto { X = 9, Y = 3 },
                DropoffPosition = new PositionDto { X = 8, Y = 6 },
                ServicePosition = new PositionDto { X = 1, Y = 0 },

                Shelves = new List<ShelfDto>
                {
                    new ShelfDto { ShelfId = "S1", Position = new PositionDto { X = 7, Y = 0 } },
                    new ShelfDto { ShelfId = "S2", Position = new PositionDto { X = 3, Y = 1 } },
                    new ShelfDto { ShelfId = "S3", Position = new PositionDto { X = 0, Y = 2 } },
                    new ShelfDto { ShelfId = "S4", Position = new PositionDto { X = 4, Y = 4 } },
                    new ShelfDto { ShelfId = "S5", Position = new PositionDto { X = 0, Y = 5 } }
                }
            };
        }
    }
}