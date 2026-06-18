using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Employee, EmployeeDto>().ReverseMap();
        CreateMap<EmployeeSkill, EmployeeSkillDto>().ReverseMap();
        CreateMap<ProjectRequest, ProjectRequestDto>();
        CreateMap<AiProjectAnalysis, AiProjectAnalysisResultDto>().ReverseMap();
    }
}
