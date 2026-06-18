using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Data.Persistence;
using Data.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.AI;
using Infrastructure.Auth;
using Infrastructure.Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Project Intake API", Version = "v1" });
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() });
});
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityCore<AppUser>(x => { x.Password.RequiredLength = 6; x.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddSignInManager();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IProjectRequestRepository, ProjectRequestRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProjectRequestService, ProjectRequestService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IMailSenderService, DevelopmentMailSender>();
builder.Services.AddHttpClient<IAiProjectAnalyzerService, AiProjectAnalyzerService>(client =>
{
    // A short timeout keeps a live demo responsive; failures automatically use the local analyzer.
    client.Timeout = TimeSpan.FromSeconds(12);
}).SetHandlerLifetime(TimeSpan.FromMinutes(5));
builder.Services.AddCors(x => x.AddPolicy("Frontend", p => p.WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow })).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DemoData.SeedAsync(scope.ServiceProvider);
}

app.Run();

static class DemoData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Client", "Admin", "SystemUser" }) if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        var users = services.GetRequiredService<UserManager<AppUser>>();
        await EnsureUser(users, "admin@demo.local", "Demo Admin", "Admin123!", "Admin");
        await EnsureUser(users, "client@demo.local", "Demo Client", "Client123!", "Client");

        var db = services.GetRequiredService<AppDbContext>();
        if (!await db.Employees.AnyAsync())
        {
            db.Employees.AddRange(
                new Employee { FullName = "Elena Petrova", Position = "Senior .NET Developer", SeniorityLevel = SeniorityLevel.Senior, MainTechStack = ".NET, C#, SQL", AdditionalSkills = "Azure, architecture", CapabilityDescription = "Backend architecture and delivery leadership", HourlyRate = 65, WeeklyAvailableHours = 28, Skills = [new() { SkillName = "ASP.NET Core", KnowledgeLevel = KnowledgeLevel.Expert, YearsOfExperience = 8 }] },
                new Employee { FullName = "Marko Stojanov", Position = "Mid React Developer", SeniorityLevel = SeniorityLevel.Mid, MainTechStack = "React, Next.js, TypeScript", AdditionalSkills = "Material UI, UX", CapabilityDescription = "Accessible responsive frontend implementation", HourlyRate = 48, WeeklyAvailableHours = 32, Skills = [new() { SkillName = "React", KnowledgeLevel = KnowledgeLevel.Advanced, YearsOfExperience = 4 }] },
                new Employee { FullName = "Ana Ilievska", Position = "Junior QA Tester", SeniorityLevel = SeniorityLevel.Junior, MainTechStack = "Manual QA, Playwright", AdditionalSkills = "API testing", CapabilityDescription = "Test planning and regression testing", HourlyRate = 28, WeeklyAvailableHours = 30, Skills = [new() { SkillName = "Quality Assurance", KnowledgeLevel = KnowledgeLevel.Intermediate, YearsOfExperience = 2 }] });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureUser(UserManager<AppUser> manager, string email, string name, string password, string role)
    {
        var user = await manager.FindByEmailAsync(email);
        if (user is null) { user = new AppUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true }; var result = await manager.CreateAsync(user, password); if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Description))); }
        if (!await manager.IsInRoleAsync(user, role)) await manager.AddToRoleAsync(user, role);
    }
}

public partial class Program;
