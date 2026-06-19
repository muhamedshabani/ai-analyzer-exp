using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController, Route("api/project-requests"), Authorize]
public sealed class ProjectRequestsController(IProjectRequestService service) : ControllerBase
{
    /// <summary>Returns all requests for admins or only the authenticated client's requests.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProjectRequestDto>>> GetAll(CancellationToken ct)
    {
        var clientId = User.IsInRole("Admin") ? null : User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(await service.GetAllAsync(clientId, User.IsInRole("Admin"), ct));
    }

    /// <summary>Returns one project request and its permitted AI analysis fields.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectRequestDto>> Get(int id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole("Admin");
        var item = await service.GetAsync(id, isAdmin, ct);
        if (item is null) return NotFound();
        if (!isAdmin)
        {
            var ownItems = await service.GetAllAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), false, ct);
            if (ownItems.All(x => x.Id != id)) return Forbid();
        }
        return Ok(item);
    }

    /// <summary>Submits a new project request for the authenticated client.</summary>
    [HttpPost, Authorize(Roles = "Client")]
    public async Task<ActionResult<ProjectRequestDto>> Create(CreateProjectRequestDto dto, CancellationToken ct)
    {
        var item = await service.CreateAsync(dto, User.FindFirstValue(ClaimTypes.NameIdentifier)!, User.FindFirstValue(ClaimTypes.Name) ?? "Client", ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    /// <summary>Analyzes a project request using AI or the stable local fallback.</summary>
    /// <remarks>This operation always returns a demo-safe analysis when the external AI service is unavailable.</remarks>
    [HttpPost("{id:int}/analyze"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<AiProjectAnalysisResultDto>> Analyze(int id, CancellationToken ct)
    {
        var result = await service.AnalyzeAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Simulates sending the generated client reply and marks it as sent.</summary>
    [HttpPost("{id:int}/send-reply"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendReply(int id, CancellationToken ct)
    {
        var sentAt = await service.SendReplyAsync(id, ct);
        return sentAt is null ? BadRequest(new { message = "Request not found or it has not been analyzed." }) : Ok(new { message = "Reply simulated successfully.", sentAt });
    }
}
