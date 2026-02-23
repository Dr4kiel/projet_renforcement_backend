using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Of;
using server.Services.Interfaces;

namespace server.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class OfsController : ControllerBase
{
    private readonly IOfService _ofService;

    public OfsController(IOfService ofService)
    {
        _ofService = ofService;
    }

    /// <summary>
    /// Get all OFs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OfDto>))]
    public async Task<IActionResult> GetAll()
    {
        var ofs = await _ofService.GetAllOfsAsync();
        return Ok(ofs);
    }

    /// <summary>
    /// Get OF by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OfDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var of = await _ofService.GetOfByIdAsync(id);
        if (of == null)
            return NotFound(new { message = $"OF with ID {id} not found" });

        return Ok(of);
    }

    /// <summary>
    /// Create new OF
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OfDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOfRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var of = await _ofService.CreateOfAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = of.Id }, of);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update existing OF
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OfDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var of = await _ofService.UpdateOfAsync(id, request);
            if (of == null)
                return NotFound(new { message = $"OF with ID {id} not found" });

            return Ok(of);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete OF
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _ofService.DeleteOfAsync(id);
            if (!result)
                return NotFound(new { message = $"OF with ID {id} not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
