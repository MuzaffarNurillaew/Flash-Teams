using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Dtos.Auth;
using Google.Apis.Auth;

namespace FlashTeams.BusinessLogic.Interfaces;

public interface IAuthService
{
    string GenerateToken(User user);

    string GenerateToken(GoogleJsonWebSignature.Payload payload);

    Task<GoogleAuthResultDto> AuthenticateAsync(GoogleAuthCredential credential, CancellationToken cancellationToken);

    string GenerateTokenBasedOn(User user, LoginDto loginDto);

    Task<string> GenerateTokenBasedOnAsync(LoginDto loginDto, CancellationToken cancellationToken);

    string? GetClaim(string claimName, bool throwExceptionIfNotFound = false);
}