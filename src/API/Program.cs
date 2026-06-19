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
    x.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Project Intake & Estimation API",
        Version = "v1",
        Description = "Backend API for client project intake, employee capacity management, AI-assisted estimation, and simulated client replies. Use the seeded demo accounts to obtain a JWT token from /api/auth/login.",
        Contact = new OpenApiContact { Name = "Master Thesis Demo" }
    });
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the JWT returned by POST /api/auth/login. Swagger adds the Bearer prefix automatically.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() });
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    x.IncludeXmlComments(xmlPath);
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
builder.Services.AddCors(x => x.AddPolicy("Frontend", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(x =>
{
    x.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Project Intake API v1");
    x.DocumentTitle = "AI Project Intake API Documentation";
    x.DisplayRequestDuration();
    x.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    x.EnableTryItOutByDefault();
});
app.UseCors("Frontend");
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow })).AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

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
