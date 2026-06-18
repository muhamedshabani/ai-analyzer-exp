using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public sealed class EmployeeService(IEmployeeRepository repository, IMapper mapper) : IEmployeeService
{
    public async Task<List<EmployeeDto>> GetAllAsync(CancellationToken ct = default) => mapper.Map<List<EmployeeDto>>(await repository.GetAllAsync(ct));
    public async Task<EmployeeDto?> GetAsync(int id, CancellationToken ct = default) => mapper.Map<EmployeeDto?>(await repository.GetAsync(id, ct));
    public async Task<EmployeeDto> CreateAsync(EmployeeDto dto, CancellationToken ct = default)
    {
        var entity = mapper.Map<Employee>(dto); entity.Id = 0; entity.CreatedAt = DateTime.UtcNow;
        foreach (var skill in entity.Skills) skill.Id = 0;
        return mapper.Map<EmployeeDto>(await repository.AddAsync(entity, ct));
    }
    public async Task<bool> UpdateAsync(int id, EmployeeDto dto, CancellationToken ct = default)
    {
        var entity = await repository.GetAsync(id, ct); if (entity is null) return false;
        mapper.Map(dto, entity); entity.Id = id; await repository.UpdateAsync(entity, ct); return true;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await repository.GetAsync(id, ct); if (entity is null) return false;
        await repository.DeleteAsync(entity, ct); return true;
    }
}
