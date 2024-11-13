using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Dtos.Auth;
using FlashTeams.Shared.Dtos.Users;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0046
namespace FlashTeams.Api.Controllers;

[Route("api/")]
[ApiController]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        string token = await authService.GenerateTokenBasedOnAsync(loginDto, cancellationToken);
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

        var token = authService.GenerateToken(user);
        return Ok(new AuthResultDto(token));
    }
}