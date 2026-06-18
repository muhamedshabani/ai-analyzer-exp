using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.AI;

public sealed class AiProjectAnalyzerService(HttpClient http, IConfiguration configuration, ILogger<AiProjectAnalyzerService> logger) : IAiProjectAnalyzerService
{
    public async Task<AiProjectAnalysisResultDto> AnalyzeProjectAsync(ProjectRequest request, List<Employee> employees, CancellationToken ct = default)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) return Mock(request, employees);

        try
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
                    new { role = "system", content = "You are an AI software project analyst. Return JSON only, with camelCase properties matching: projectSummary, functionalRequirements, suggestedModules, suggestedTechStack, clarificationQuestions, complexityLevel (Low|Medium|High|VeryHigh), estimatedTimeline, estimatedCostRange, recommendedTeam, risksAndAssumptions, clientReplyDraft, internalNotes. Use readable newline-separated text for list-like fields." },
                    new { role = "user", content = JsonSerializer.Serialize(new { project = new { request.ClientName, request.CompanyName, request.ProjectTitle, request.ProjectDescription, request.BudgetRange, request.DesiredDeadline, request.Industry }, availableEmployees = employees.Where(x => x.IsAvailable).Select(x => new { x.FullName, x.Position, x.SeniorityLevel, x.MainTechStack, x.AdditionalSkills, x.HourlyRate, x.WeeklyAvailableHours, skills = x.Skills.Select(s => new { s.SkillName, s.KnowledgeLevel, s.YearsOfExperience }) }) }) }
                }
            });
            var response = await http.SendAsync(message, ct);
            response.EnsureSuccessStatusCode();
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var content = json.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return JsonSerializer.Deserialize<AiProjectAnalysisResultDto>(content!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } }) ?? Mock(request, employees);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI analysis failed; returning stable demo analysis.");
            return Mock(request, employees);
        }
    }

    private static AiProjectAnalysisResultDto Mock(ProjectRequest request, List<Employee> employees)
    {
        var team = employees.Where(x => x.IsAvailable).OrderByDescending(x => x.SeniorityLevel).Take(3).ToList();
        var weeklyCost = team.Sum(x => x.HourlyRate * x.WeeklyAvailableHours);
        return new AiProjectAnalysisResultDto
        {
            ProjectSummary = $"A web-based {request.Industry} solution for {request.CompanyName.DefaultIfEmpty("the client")} based on the submitted project brief.",
            FunctionalRequirements = "Secure user authentication\nRole-based dashboards\nCore data management\nSearch and reporting\nEmail notifications",
            SuggestedModules = "Authentication\nClient portal\nAdministration\nReporting\nNotifications",
            SuggestedTechStack = "ASP.NET Core Web API, Entity Framework Core, SQLite, Next.js, TypeScript, Material UI",
            ClarificationQuestions = "Who are the primary user roles?\nWhich integrations are mandatory for the first release?\nWhat data must be included in reports?",
            ComplexityLevel = ComplexityLevel.Medium,
            EstimatedTimeline = "8–12 weeks for a focused MVP",
            EstimatedCostRange = weeklyCost > 0 ? $"€{weeklyCost * 8:N0}–€{weeklyCost * 12:N0}" : "€25,000–€40,000",
            RecommendedTeam = team.Count > 0 ? string.Join("\n", team.Select(x => $"{x.FullName} — {x.Position} ({x.WeeklyAvailableHours}h/week)")) : "One senior full-stack developer and one QA engineer",
            RisksAndAssumptions = "Estimate assumes a focused MVP, timely feedback, and no complex third-party integrations. Scope changes may affect cost and schedule.",
            ClientReplyDraft = $"Hello {request.ClientName},\n\nThank you for sharing “{request.ProjectTitle}”. We reviewed the initial scope and estimate an 8–12 week MVP. Before finalizing a proposal, we would like to clarify the key user roles, required integrations, and reporting needs.\n\nKind regards,\nProject Intake Team",
            InternalNotes = "Demo fallback analysis generated locally. Review estimates before sending."
        };
    }
}

file static class StringExtensions
{
    public static string DefaultIfEmpty(this string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
