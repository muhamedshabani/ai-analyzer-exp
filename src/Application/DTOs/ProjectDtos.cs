using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed class CreateProjectRequestDto
{
    [Required] public string ProjectTitle { get; set; } = string.Empty;
    [Required, MinLength(20)] public string ProjectDescription { get; set; } = string.Empty;
    [Required] public string Industry { get; set; } = string.Empty;
    [Required] public string BudgetRange { get; set; } = string.Empty;
    public DateTime? DesiredDeadline { get; set; }
    [Required, EmailAddress] public string ContactEmail { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

public sealed class ProjectRequestDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public string BudgetRange { get; set; } = string.Empty;
    public DateTime? DesiredDeadline { get; set; }
    public string Industry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ProjectStatus Status { get; set; }
    public AiProjectAnalysisResultDto? Analysis { get; set; }
}

public sealed class AiProjectAnalysisResultDto
{
    public bool IsFallback { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
    public string ProjectSummary { get; set; } = string.Empty;
    public string FunctionalRequirements { get; set; } = string.Empty;
    public string SuggestedModules { get; set; } = string.Empty;
    public string SuggestedTechStack { get; set; } = string.Empty;
    public string ClarificationQuestions { get; set; } = string.Empty;
    public ComplexityLevel ComplexityLevel { get; set; }
    public string EstimatedTimeline { get; set; } = string.Empty;
    public string EstimatedCostRange { get; set; } = string.Empty;
    public string RecommendedTeam { get; set; } = string.Empty;
    public string RisksAndAssumptions { get; set; } = string.Empty;
    public string ClientReplyDraft { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
    public DateTime? ReplySentAt { get; set; }
}

public sealed record DashboardDto(int TotalRequests, int AnalyzedRequests, int PendingRequests, int AvailableEmployees);
