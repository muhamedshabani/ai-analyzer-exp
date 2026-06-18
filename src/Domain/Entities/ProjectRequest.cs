using Domain.Enums;

namespace Domain.Entities;

public sealed class ProjectRequest
{
    public int Id { get; set; }
    public string ClientUserId { get; set; } = string.Empty;
    public AppUser? ClientUser { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public string BudgetRange { get; set; } = string.Empty;
    public DateTime? DesiredDeadline { get; set; }
    public string Industry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ProjectStatus Status { get; set; } = ProjectStatus.Submitted;
    public AiProjectAnalysis? Analysis { get; set; }
}

public sealed class AiProjectAnalysis
{
    public int Id { get; set; }
    public int ProjectRequestId { get; set; }
    public ProjectRequest? ProjectRequest { get; set; }
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReplySentAt { get; set; }
}
