using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Line;
using server.Services.Interfaces;

namespace server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class LinesController : ControllerBase
{
    private readonly ILineService _lineService;

    public LinesController(ILineService lineService)
    {
        _lineService = lineService;
    }

    /// <summary>
    /// Get all lines
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LineDto>))]
    public async Task<IActionResult> GetAll()
    {
        var lines = await _lineService.GetAllLinesAsync();
        return Ok(lines);
    }

    /// <summary>
    /// Get line by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LineDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var line = await _lineService.GetLineByIdAsync(id);
        if (line == null)
        {
            return NotFound(new { message = $"Line with ID {id} not found" });
        }

        return Ok(line);
    }

    /// <summary>
    /// Create new line
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LineDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLineRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var line = await _lineService.CreateLineAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = line.Id }, line);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing line
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LineDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLineRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var line = await _lineService.UpdateLineAsync(id, request);
            if (line == null)
            {
                return NotFound(new { message = $"Line with ID {id} not found" });
            }

            return Ok(line);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete line
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _lineService.DeleteLineAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Line with ID {id} not found" });
        }

        return NoContent();
    }
}
