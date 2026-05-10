using Microsoft.AspNetCore.Mvc;
using RobotServer.Services;
using RobotShared;

namespace RobotServer.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    public class WarehouseController : ControllerBase
    {
        private readonly WarehouseService _warehouseService;

        public WarehouseController(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public ActionResult<WarehouseDto> GetWarehouse()
        {
            var warehouse = _warehouseService.GetWarehouse();

            return Ok(warehouse);
        }
    }
}