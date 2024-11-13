using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.Shared.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable SA1010
namespace FlashTeams.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class ThirdPartyAuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("google")]
    public async Task<ActionResult<GoogleAuthResultDto>> GoogleLogin(GoogleAuthCredential credential, CancellationToken cancellationToken)
    {
        var result = await authService.AuthenticateAsync(credential, cancellationToken);

        return Ok(result);
    }
}
