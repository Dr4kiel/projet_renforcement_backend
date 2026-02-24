using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentControlService.Services.Interfaces;

namespace AgentControlService.Controllers;

[ApiController]
[Route("api/agents")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpPost("{lineId}/start")]
    public async Task<IActionResult> StartLine(string lineId)
    {
        await _agentService.StartLineAsync(lineId);
        return Ok(new { message = $"Start command sent to line {lineId}" });
    }

    [HttpPost("{lineId}/stop")]
    public async Task<IActionResult> StopLine(string lineId)
    {
        await _agentService.StopLineAsync(lineId);
        return Ok(new { message = $"Stop command sent to line {lineId}" });
    }
}
