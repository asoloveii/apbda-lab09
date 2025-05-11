namespace apbd_lab09.Controllers;

using apbd_lab09.Models.DTOs;
using apbd_lab09.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _service;

    public WarehouseController(IWarehouseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Adds a product to the warehouse using manual SQL commands.
    /// </summary>
    [HttpPost("manual")]
    public async Task<IActionResult> AddProductManual([FromBody] ProductWarehouseDTO request)
    {
        var result = await _service.AddProductToWarehouse(request);

        if (result.IsSuccess)
            return Ok(new { Id = result.Id });

        return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Adds a product to the warehouse using a stored procedure.
    /// </summary>
    [HttpPost("stored-proc")]
    public async Task<IActionResult> AddProductStoredProcedure([FromBody] ProductWarehouseDTO request)
    {
        var result = await _service.AddProductToWarehouseUsingProcedure(request);

        if (result.IsSuccess)
            return Ok(new { Id = result.Id });

        return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
    }
}
