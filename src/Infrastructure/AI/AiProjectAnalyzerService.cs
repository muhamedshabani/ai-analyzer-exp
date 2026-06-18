using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.AI;

public sealed class AiProjectAnalyzerService(
    HttpClient http,
    IConfiguration configuration,
    ILogger<AiProjectAnalyzerService> logger) : IAiProjectAnalyzerService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<AiProjectAnalysisResultDto> AnalyzeProjectAsync(
        ProjectRequest request,
        List<Employee> employees,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        employees ??= [];

        var apiKey = FirstNonEmpty(
            configuration["OpenAI:ApiKey"],
            Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            Environment.GetEnvironmentVariable("AI_API_KEY"));

        if (apiKey is null)
        {
            logger.LogInformation("No AI API key configured; using deterministic demo analysis.");
            return CreateMockAnalysis(request, employees, "Demo mode: no AI API key is configured; this analysis was generated locally.");
        }

        try
        {
            var result = await AnalyzeWithApiAsync(request, employees, apiKey, ct);
            ValidateApiResult(result);
            result.InternalNotes = AppendNote(result.InternalNotes, "External AI analysis completed successfully. Review estimates before sending.");
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "External AI analysis failed; using deterministic demo fallback.");
            return CreateMockAnalysis(request, employees, "Warning: the external AI service was unavailable or returned invalid data; a local fallback analysis was generated.");
        }
    }

    private async Task<AiProjectAnalysisResultDto> AnalyzeWithApiAsync(
        ProjectRequest request,
        IReadOnlyCollection<Employee> employees,
        string apiKey,
        CancellationToken ct)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        message.Content = JsonContent.Create(new
        {
            model = configuration["OpenAI:Model"] ?? "gpt-4o-mini",
            response_format = new { type = "json_object" },
            temperature = 0.2,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                        You are a software project intake analyst. Return one JSON object only.
                        Use these exact camelCase properties: projectSummary, functionalRequirements,
                        suggestedModules, suggestedTechStack, clarificationQuestions, complexityLevel,
                        estimatedTimeline, estimatedCostRange, recommendedTeam, risksAndAssumptions,
                        clientReplyDraft, internalNotes. complexityLevel must be Low, Medium, High, or
                        VeryHigh. List-like properties must be readable newline-separated strings.
                        Base team selection and cost on available employee skills, seniority, hourly
                        rate, and weekly available hours. Give ballpark estimates, not guarantees.
                        Do not invent employees and never include secrets or markdown code fences.
                        """
                },
                new
                {
                    role = "user",
                    content = JsonSerializer.Serialize(new
                    {
                        project = new
                        {
                            request.ClientName,
                            request.CompanyName,
                            request.ProjectTitle,
                            request.ProjectDescription,
                            request.BudgetRange,
                            request.DesiredDeadline,
                            request.Industry
                        },
                        availableEmployees = employees.Where(x => x.IsAvailable).Select(x => new
                        {
                            x.Id,
                            x.FullName,
                            x.Position,
                            x.SeniorityLevel,
                            x.MainTechStack,
                            x.AdditionalSkills,
                            x.CapabilityDescription,
                            x.HourlyRate,
                            x.WeeklyAvailableHours,
                            skills = x.Skills.Select(s => new
                            {
                                s.SkillName,
                                s.KnowledgeLevel,
                                s.YearsOfExperience
                            })
                        })
                    }, JsonOptions)
                }
            }
        });

        using var response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return JsonSerializer.Deserialize<AiProjectAnalysisResultDto>(content ?? string.Empty, JsonOptions)
            ?? throw new InvalidOperationException("AI service returned an empty analysis.");
    }

    private static AiProjectAnalysisResultDto CreateMockAnalysis(
        ProjectRequest request,
        IReadOnlyCollection<Employee> employees,
        string note)
    {
        var projectText = $"{request.ProjectTitle} {request.ProjectDescription} {request.Industry}".ToLowerInvariant();
        var complexity = EstimateComplexity(projectText);
        var (minimumWeeks, maximumWeeks) = complexity switch
        {
            ComplexityLevel.Low => (4, 6),
            ComplexityLevel.Medium => (8, 12),
            ComplexityLevel.High => (12, 18),
            _ => (18, 26)
        };

        var team = SelectTeam(employees, projectText);
        var weeklyTeamCost = team.Sum(x => x.HourlyRate * x.WeeklyAvailableHours);
        var minimumCost = weeklyTeamCost > 0 ? weeklyTeamCost * minimumWeeks * 0.75m : 18_000m;
        var maximumCost = weeklyTeamCost > 0 ? weeklyTeamCost * maximumWeeks * 0.90m : 35_000m;
        var company = string.IsNullOrWhiteSpace(request.CompanyName) ? "the client" : request.CompanyName;
        var clientName = string.IsNullOrWhiteSpace(request.ClientName) ? "there" : request.ClientName;
        var modules = SuggestedModules(projectText);

        return new AiProjectAnalysisResultDto
        {
            ProjectSummary = $"{request.ProjectTitle} is a proposed {request.Industry} solution for {company}. The first release should focus on the core user journey, secure administration, and measurable operational value.",
            FunctionalRequirements = string.Join('\n', new[]
            {
                "Secure authentication and role-based access",
                "Client-facing workflow for the primary project use case",
                "Administrative management of users and business data",
                "Validation, search, filtering, and status tracking",
                "Audit-friendly reporting and email notifications"
            }),
            SuggestedModules = string.Join('\n', modules),
            SuggestedTechStack = BuildSuggestedTechStack(team),
            ClarificationQuestions = string.Join('\n', new[]
            {
                "Which user roles and permissions are required for the MVP?",
                "Which integrations are mandatory for the first release?",
                "What information must appear in dashboards and exported reports?",
                "Is the desired deadline fixed, and what scope can be deferred if necessary?",
                $"Is the stated budget range ({request.BudgetRange.DefaultIfEmpty("not provided")}) flexible after requirements are confirmed?"
            }),
            ComplexityLevel = complexity,
            EstimatedTimeline = $"{minimumWeeks}–{maximumWeeks} weeks for a focused MVP, including testing and stakeholder review",
            EstimatedCostRange = $"€{Math.Round(minimumCost / 500m) * 500m:N0}–€{Math.Round(maximumCost / 500m) * 500m:N0}",
            RecommendedTeam = team.Count > 0
                ? string.Join('\n', team.Select(x => $"{x.FullName} — {x.Position}; {x.SeniorityLevel}; {x.WeeklyAvailableHours}h/week at €{x.HourlyRate:N0}/h; relevant skills: {EmployeeSkills(x)}"))
                : "No available employees were found. Suggested minimum staffing: one senior full-stack developer, one frontend developer, and part-time QA support.",
            RisksAndAssumptions = string.Join('\n', new[]
            {
                "Estimate assumes an MVP scope and timely stakeholder feedback.",
                "Complex integrations, data migration, compliance, or major scope changes will affect cost and schedule.",
                "Employee allocation is based on current weekly availability and must be reconfirmed before kickoff.",
                "The estimate is a ballpark range, not a binding commercial proposal."
            }),
            ClientReplyDraft = $"Hello {clientName},\n\nThank you for sharing “{request.ProjectTitle}”. We completed an initial review and estimate a {minimumWeeks}–{maximumWeeks} week MVP with a ballpark cost of €{Math.Round(minimumCost / 500m) * 500m:N0}–€{Math.Round(maximumCost / 500m) * 500m:N0}. Before preparing a final proposal, we would like to confirm the user roles, required integrations, reporting needs, and deadline flexibility.\n\nKind regards,\nProject Intake Team",
            InternalNotes = $"{note} Complexity and cost are deterministic demo estimates calculated from scope signals and current employee capacity. Review before sending."
        };
    }

    private static List<Employee> SelectTeam(IReadOnlyCollection<Employee> employees, string projectText)
    {
        return employees
            .Where(x => x.IsAvailable && x.WeeklyAvailableHours > 0)
            .Select(x => new
            {
                Employee = x,
                Score = SkillMatchScore(x, projectText)
                    + (int)x.SeniorityLevel * 2
                    + Math.Min(x.WeeklyAvailableHours, 40) / 10.0
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Employee.HourlyRate)
            .Take(3)
            .Select(x => x.Employee)
            .ToList();
    }

    private static double SkillMatchScore(Employee employee, string projectText)
    {
        var terms = $"{employee.Position} {employee.MainTechStack} {employee.AdditionalSkills} {employee.CapabilityDescription} {string.Join(' ', employee.Skills.Select(x => x.SkillName))}"
            .ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '/', '-', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 3)
            .Distinct();
        return terms.Sum(term => projectText.Contains(term, StringComparison.Ordinal) ? 3d : 0d)
            + employee.Skills.Sum(x => (int)x.KnowledgeLevel * 0.5 + Math.Min(x.YearsOfExperience, 10) * 0.1);
    }

    private static ComplexityLevel EstimateComplexity(string projectText)
    {
        var highRiskSignals = new[] { "real-time", "realtime", "payment", "integration", "migration", "mobile", "multi-tenant", "machine learning", " artificial intelligence", "compliance", "marketplace" };
        var signalCount = highRiskSignals.Count(projectText.Contains);
        var wordCount = projectText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return (signalCount, wordCount) switch
        {
            (>= 6, _) or (_, >= 500) => ComplexityLevel.VeryHigh,
            (>= 3, _) or (_, >= 250) => ComplexityLevel.High,
            (>= 1, _) or (_, >= 90) => ComplexityLevel.Medium,
            _ => ComplexityLevel.Low
        };
    }

    private static List<string> SuggestedModules(string projectText)
    {
        var modules = new List<string> { "Authentication and access control", "Core business workflow", "Administration", "Reporting and notifications" };
        if (projectText.Contains("payment")) modules.Add("Payments and billing");
        if (projectText.Contains("integration") || projectText.Contains("api")) modules.Add("External integrations");
        if (projectText.Contains("mobile")) modules.Add("Mobile experience");
        return modules;
    }

    private static string BuildSuggestedTechStack(IReadOnlyCollection<Employee> team)
    {
        var stacks = team.Select(x => x.MainTechStack).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        return stacks.Count > 0
            ? $"Team-aligned stack: {string.Join("; ", stacks)}. Suggested platform: ASP.NET Core Web API, EF Core, SQLite for the demo, and a TypeScript web client."
            : "ASP.NET Core Web API, EF Core, SQLite, and a TypeScript web client.";
    }

    private static string EmployeeSkills(Employee employee)
    {
        var skills = employee.Skills.Select(x => $"{x.SkillName} ({x.KnowledgeLevel})").ToList();
        return skills.Count > 0 ? string.Join(", ", skills) : employee.MainTechStack.DefaultIfEmpty("general delivery");
    }

    private static void ValidateApiResult(AiProjectAnalysisResultDto result)
    {
        var required = new[]
        {
            result.ProjectSummary, result.FunctionalRequirements, result.SuggestedModules,
            result.EstimatedTimeline, result.EstimatedCostRange, result.RecommendedTeam,
            result.ClarificationQuestions, result.RisksAndAssumptions, result.ClientReplyDraft
        };
        if (required.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("AI service returned an incomplete analysis.");
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

    private static string AppendNote(string existing, string note) =>
        string.IsNullOrWhiteSpace(existing) ? note : $"{existing.Trim()} {note}";
}

file static class StringExtensions
{
    public static string DefaultIfEmpty(this string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
