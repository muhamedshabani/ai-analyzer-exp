using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed record RegisterDto([Required] string FullName, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public sealed record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
public sealed record AuthResponseDto(string Token, string FullName, string Email, IReadOnlyCollection<string> Roles);
