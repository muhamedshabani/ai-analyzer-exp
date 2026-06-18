using Domain.Enums;

namespace Domain.Entities;

public sealed class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public SeniorityLevel SeniorityLevel { get; set; }
    public string MainTechStack { get; set; } = string.Empty;
    public string AdditionalSkills { get; set; } = string.Empty;
    public string CapabilityDescription { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int WeeklyAvailableHours { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<EmployeeSkill> Skills { get; set; } = [];
}

public sealed class EmployeeSkill
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public KnowledgeLevel KnowledgeLevel { get; set; }
    public double YearsOfExperience { get; set; }
}
