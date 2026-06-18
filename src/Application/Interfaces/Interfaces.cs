using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee?> GetAsync(int id, CancellationToken ct = default);
    Task<Employee> AddAsync(Employee entity, CancellationToken ct = default);
    Task UpdateAsync(Employee entity, CancellationToken ct = default);
    Task DeleteAsync(Employee entity, CancellationToken ct = default);
}

public interface IProjectRequestRepository
{
    Task<List<ProjectRequest>> GetAllAsync(string? clientUserId = null, CancellationToken ct = default);
    Task<ProjectRequest?> GetAsync(int id, CancellationToken ct = default);
    Task<ProjectRequest> AddAsync(ProjectRequest entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IAiProjectAnalyzerService
{
    Task<AiProjectAnalysisResultDto> AnalyzeProjectAsync(ProjectRequest request, List<Employee> employees, CancellationToken ct = default);
}

public interface IJwtTokenService
{
    string CreateToken(AppUser user, IList<string> roles);
}

public interface IMailSenderService
{
    Task SendAsync(string recipient, string subject, string body, CancellationToken ct = default);
}

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(CancellationToken ct = default);
    Task<EmployeeDto?> GetAsync(int id, CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(EmployeeDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, EmployeeDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IProjectRequestService
{
    Task<List<ProjectRequestDto>> GetAllAsync(string? clientUserId, bool includeInternalAnalysis, CancellationToken ct = default);
    Task<ProjectRequestDto?> GetAsync(int id, bool includeInternalAnalysis, CancellationToken ct = default);
    Task<ProjectRequestDto> CreateAsync(CreateProjectRequestDto dto, string userId, string clientName, CancellationToken ct = default);
    Task<AiProjectAnalysisResultDto?> AnalyzeAsync(int id, CancellationToken ct = default);
    Task<DateTime?> SendReplyAsync(int id, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
