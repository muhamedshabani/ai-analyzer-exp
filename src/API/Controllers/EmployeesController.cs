using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("api/employees"), Authorize(Roles = "Admin")]
public sealed class EmployeesController(IEmployeeService service) : ControllerBase
{
    /// <summary>Returns all employees and their skills.</summary>
    [HttpGet] public async Task<ActionResult<List<EmployeeDto>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    /// <summary>Returns one employee by identifier.</summary>
    [HttpGet("{id:int}")] public async Task<ActionResult<EmployeeDto>> Get(int id, CancellationToken ct) { var item = await service.GetAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    /// <summary>Creates an employee with skills, rate, and weekly availability.</summary>
    [HttpPost] public async Task<ActionResult<EmployeeDto>> Create(EmployeeDto dto, CancellationToken ct) { var item = await service.CreateAsync(dto, ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, item); }
    /// <summary>Updates an existing employee.</summary>
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, EmployeeDto dto, CancellationToken ct) => await service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();
    /// <summary>Deletes an employee.</summary>
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
