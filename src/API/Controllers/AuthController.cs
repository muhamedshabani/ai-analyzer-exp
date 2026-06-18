using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(UserManager<AppUser> users, SignInManager<AppUser> signIn, IJwtTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var user = new AppUser { FullName = dto.FullName, Email = dto.Email, UserName = dto.Email };
        var result = await users.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(x => x.Description));
        await users.AddToRoleAsync(user, "Client");
        return Ok(await Response(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await users.FindByEmailAsync(dto.Email);
        if (user is null || !(await signIn.CheckPasswordSignInAsync(user, dto.Password, true)).Succeeded) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(await Response(user));
    }

    private async Task<AuthResponseDto> Response(AppUser user)
    {
        var roles = await users.GetRolesAsync(user);
        return new(tokens.CreateToken(user, roles), user.FullName, user.Email!, roles.ToArray());
    }
}
