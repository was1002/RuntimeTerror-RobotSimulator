using Microsoft.AspNetCore.Mvc;
using RobotShared;
using System.Collections.Generic;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    public class WarehouseController : ControllerBase
    {
        [HttpGet]
        public ActionResult<WarehouseDto> GetWarehouse()
        {
            // Dummy warehouse adatok az 1-es verzióú leírás alapján (10x7).
            var warehouse = new WarehouseDto
            {
                Width = 10,
                Height = 7,
                SpawnPosition = new PositionDto { X = 7, Y = 2 }, // 'R' pozíció
                ChargerPosition = new PositionDto { X = 9, Y = 3 }, // 'C'
                DropoffPosition = new PositionDto { X = 8, Y = 6 }, // 'D'
                ServicePosition = new PositionDto { X = 2, Y = 0 }, // 'W'
                Shelves = new List<ShelfDto>
                {
                    new ShelfDto { ShelfId = "S1", Position = new PositionDto { X = 7, Y = 0 } },
                    new ShelfDto { ShelfId = "S2", Position = new PositionDto { X = 3, Y = 1 } },
                    new ShelfDto { ShelfId = "S3", Position = new PositionDto { X = 0, Y = 2 } },
                    new ShelfDto { ShelfId = "S4", Position = new PositionDto { X = 4, Y = 4 } },
                    new ShelfDto { ShelfId = "S5", Position = new PositionDto { X = 0, Y = 5 } }
                }
            };

            return Ok(warehouse);
        }
    }
}