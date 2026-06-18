using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
