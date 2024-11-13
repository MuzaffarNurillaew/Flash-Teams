using System.Data;
using System.Security.Claims;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Dtos.Auth;
using FlashTeams.Shared.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashTeams.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController(IUserService userService, IAuthService authService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<UserResultDto>> Create(UserCreationDto userDto, CancellationToken cancellationToken)
    {
        var user = new User(userDto.FirstName, userDto.LastName, userDto.Email, userDto.Username, userDto.PhoneNumber, userDto.Password);

        var createdUser = await userService.CreateAsync(user, cancellationToken);

        return Ok(new UserResultDto(createdUser));
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResultDto>>> GetAll(CancellationToken cancellationToken)
    {
        var foundUsers = await userService.GetAllAsync(cancellationToken);

        var mappedUsers = foundUsers.Select(UserResultDto.Create);

        return Ok(mappedUsers);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResultDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var foundUser = await userService.GetByIdAsync(id, cancellationToken);

        return Ok(UserResultDto.Create(foundUser));
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<bool>> DeleteById(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await userService.DeleteByIdAsync(id, cancellationToken);

        return Ok(isDeleted);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<UserResultDto>> Update(UserUpdateDto userDto, CancellationToken cancellationToken)
    {
        var user = new User(
            userDto.Id,
            userDto.FirstName,
            userDto.LastName,
            email: default,
            username: userDto.Username,
            phoneNumber: default,
            userDto.Password);

        var updatedUser = await userService.UpdateAsync(user, cancellationToken);

        return Ok(UserResultDto.Create(updatedUser));
    }

    [Authorize]
    [HttpPost("set-password-first-time")]
    public async Task<ActionResult<bool>> SetPasswordFirstTime(FirstTimePasswordCreationDto setPasswordFirstTimeDto, CancellationToken cancellationToken)
    {
        string email = authService.GetClaim(ClaimTypes.Email, throwExceptionIfNotFound: true);

        bool isPasswordSet = await userService.SetPasswordFirstTimeAsync(email!, setPasswordFirstTimeDto, cancellationToken);

        return Ok(isPasswordSet);
    }
}