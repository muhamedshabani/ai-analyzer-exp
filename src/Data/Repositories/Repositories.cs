using Application.Interfaces;
using Data.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public Task<List<Employee>> GetAllAsync(CancellationToken ct = default) => db.Employees.Include(x => x.Skills).AsNoTracking().OrderBy(x => x.FullName).ToListAsync(ct);
    public Task<Employee?> GetAsync(int id, CancellationToken ct = default) => db.Employees.Include(x => x.Skills).FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Employee> AddAsync(Employee entity, CancellationToken ct = default) { db.Employees.Add(entity); await db.SaveChangesAsync(ct); return entity; }
    public async Task UpdateAsync(Employee entity, CancellationToken ct = default) { await db.SaveChangesAsync(ct); }
    public async Task DeleteAsync(Employee entity, CancellationToken ct = default) { db.Employees.Remove(entity); await db.SaveChangesAsync(ct); }
}

public sealed class ProjectRequestRepository(AppDbContext db) : IProjectRequestRepository
{
    public Task<List<ProjectRequest>> GetAllAsync(string? clientUserId = null, CancellationToken ct = default)
    {
        var query = db.ProjectRequests.Include(x => x.Analysis).AsNoTracking().AsQueryable();
        if (clientUserId is not null) query = query.Where(x => x.ClientUserId == clientUserId);
        return query.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    }

    public Task<ProjectRequest?> GetAsync(int id, CancellationToken ct = default) => db.ProjectRequests.Include(x => x.Analysis).FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<ProjectRequest> AddAsync(ProjectRequest entity, CancellationToken ct = default) { db.ProjectRequests.Add(entity); await db.SaveChangesAsync(ct); return entity; }
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
