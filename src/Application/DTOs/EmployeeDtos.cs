using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed class EmployeeDto
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public string Position { get; set; } = string.Empty;
    public SeniorityLevel SeniorityLevel { get; set; }
    [Required] public string MainTechStack { get; set; } = string.Empty;
    public string AdditionalSkills { get; set; } = string.Empty;
    public string CapabilityDescription { get; set; } = string.Empty;
    [Range(0, 1000)] public decimal HourlyRate { get; set; }
    [Range(0, 40)] public int WeeklyAvailableHours { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<EmployeeSkillDto> Skills { get; set; } = [];
}

public sealed class EmployeeSkillDto
{
    public int Id { get; set; }
    [Required] public string SkillName { get; set; } = string.Empty;
    public KnowledgeLevel KnowledgeLevel { get; set; }
    [Range(0, 60)] public double YearsOfExperience { get; set; }
}
