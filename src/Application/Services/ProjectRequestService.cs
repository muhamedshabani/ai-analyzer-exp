using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ProjectRequestService(IProjectRequestRepository projects, IEmployeeRepository employees, IAiProjectAnalyzerService analyzer, IMailSenderService mail, IMapper mapper) : IProjectRequestService
{
    public async Task<List<ProjectRequestDto>> GetAllAsync(string? clientUserId, bool includeInternalAnalysis, CancellationToken ct = default)
    {
        var result = mapper.Map<List<ProjectRequestDto>>(await projects.GetAllAsync(clientUserId, ct));
        if (!includeInternalAnalysis) result.ForEach(HideInternalAnalysis);
        return result;
    }
    public async Task<ProjectRequestDto?> GetAsync(int id, bool includeInternalAnalysis, CancellationToken ct = default)
    {
        var result = mapper.Map<ProjectRequestDto?>(await projects.GetAsync(id, ct));
        if (result is not null && !includeInternalAnalysis) HideInternalAnalysis(result);
        return result;
    }
    public async Task<ProjectRequestDto> CreateAsync(CreateProjectRequestDto dto, string userId, string clientName, CancellationToken ct = default)
    {
        var entity = new ProjectRequest { ClientUserId = userId, ClientName = clientName, ClientEmail = dto.ContactEmail, CompanyName = dto.CompanyName, ProjectTitle = dto.ProjectTitle, ProjectDescription = dto.ProjectDescription, BudgetRange = dto.BudgetRange, DesiredDeadline = dto.DesiredDeadline, Industry = dto.Industry };
        return mapper.Map<ProjectRequestDto>(await projects.AddAsync(entity, ct));
    }
    public async Task<AiProjectAnalysisResultDto?> AnalyzeAsync(int id, CancellationToken ct = default)
    {
        var entity = await projects.GetAsync(id, ct); if (entity is null) return null;
        var result = await analyzer.AnalyzeProjectAsync(entity, await employees.GetAllAsync(ct), ct);
        // The analyzer keeps its deterministic fallback for demo resilience, but a fallback
        // must never overwrite or masquerade as a real model analysis in the application.
        if (result.IsFallback) return result;
        entity.Analysis ??= new AiProjectAnalysis { ProjectRequestId = entity.Id };
        mapper.Map(result, entity.Analysis); entity.Status = ProjectStatus.Analyzed; await projects.SaveChangesAsync(ct); return result;
    }
    public async Task<DateTime?> SendReplyAsync(int id, CancellationToken ct = default)
    {
        var entity = await projects.GetAsync(id, ct); if (entity?.Analysis is null) return null;
        await mail.SendAsync(entity.ClientEmail, $"Your project request: {entity.ProjectTitle}", entity.Analysis.ClientReplyDraft, ct);
        entity.Analysis.ReplySentAt = DateTime.UtcNow; entity.Status = ProjectStatus.ProposalSent; await projects.SaveChangesAsync(ct); return entity.Analysis.ReplySentAt;
    }
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var requests = await projects.GetAllAsync(null, ct); var people = await employees.GetAllAsync(ct);
        return new(requests.Count, requests.Count(x => x.Analysis is not null), requests.Count(x => x.Status is ProjectStatus.Submitted or ProjectStatus.InReview), people.Count(x => x.IsAvailable));
    }

    private static void HideInternalAnalysis(ProjectRequestDto request)
    {
        if (request.Analysis is null) return;
        request.Analysis.InternalNotes = string.Empty;
        request.Analysis.ClientReplyDraft = string.Empty;
        request.Analysis.RecommendedTeam = string.Empty;
    }
}
