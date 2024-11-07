using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Configurations;
using FlashTeams.Shared.Dtos.Auth;
using FlashTeams.Shared.Dtos.Users;
using FlashTeams.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable IDE0046
namespace FlashTeams.Api.Controllers;

[Route("api/")]
[ApiController]
public class AuthController(IUserService userService, JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await userService.GetByEmailAsync(loginDto.Email, false, cancellationToken)
            ?? throw new FlashTeamsException(401, "Login or password is incorrect!");

        string token = GenerateToken(user, loginDto);
        return Ok(new AuthResultDto(token));
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResultDto>> Signup(UserCreationDto signupDto, CancellationToken cancellationToken)
    {
        var user = await userService.CreateAsync(
            new User(
                signupDto.FirstName,
                signupDto.LastName,
                signupDto.Email,
                signupDto.Username,
                signupDto.PhoneNumber,
                signupDto.Password),
            cancellationToken);

        var token = GenerateToken(user);
        return Ok(new AuthResultDto(token));
    }

    private string GenerateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new("Id", user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateToken(User user, LoginDto loginDto)
    {
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new FlashTeamsException(401, "Login or password is incorrect!");
        }

        return GenerateToken(user);
    }
}