using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("api/employees"), Authorize(Roles = "Admin")]
public sealed class EmployeesController(IEmployeeService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<List<EmployeeDto>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("{id:int}")] public async Task<ActionResult<EmployeeDto>> Get(int id, CancellationToken ct) { var item = await service.GetAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost] public async Task<ActionResult<EmployeeDto>> Create(EmployeeDto dto, CancellationToken ct) { var item = await service.CreateAsync(dto, ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, EmployeeDto dto, CancellationToken ct) => await service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
