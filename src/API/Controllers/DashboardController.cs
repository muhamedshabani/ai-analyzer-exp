using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("api/dashboard"), Authorize(Roles = "Admin")]
public sealed class DashboardController(IProjectRequestService service) : ControllerBase
{
    /// <summary>Returns admin dashboard totals for requests and employee availability.</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        return Ok(await service.GetDashboardAsync(ct));
    }
}
