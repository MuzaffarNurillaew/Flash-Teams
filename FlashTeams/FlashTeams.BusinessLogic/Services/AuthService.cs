using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Configurations;
using FlashTeams.Shared.Dtos.Auth;
using FlashTeams.Shared.Exceptions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace FlashTeams.BusinessLogic.Services;

#pragma warning disable SA1010
#pragma warning disable IDE0046
public class AuthService(JwtSettings jwtSettings, GoogleAuthSettings googleAuthSettings, IUserService userService, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
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

    public string GenerateToken(GoogleJsonWebSignature.Payload payload)
    {
        throw new NotImplementedException();
    }

    public async Task<GoogleAuthResultDto> AuthenticateAsync(GoogleAuthCredential credential, CancellationToken cancellationToken)
    {
        var clientId = googleAuthSettings.ClientId;

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(credential.Token, new()
            {
                Audience = [clientId],
            });

            var user = await userService.GetByEmailAsync(payload.Email, false, cancellationToken);

            user ??= await userService.CreateAsync(new User(payload.GivenName, payload.FamilyName, payload.Email, default, default, default, payload.Subject), cancellationToken);

            string token = GenerateToken(user);

            return new GoogleAuthResultDto(token)
            {
                IsNewUser = user.PasswordHash is null,
                Email = user.Email,
            };
        }
        catch (InvalidJwtException)
        {
            throw new FlashTeamsException(401, "Invalid Google ID token.");
        }
    }

    public string GenerateTokenBasedOn(User user, LoginDto loginDto)
    {
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new FlashTeamsException(401, "Login or password is incorrect!");
        }

        return GenerateToken(user);
    }

    public async Task<string> GenerateTokenBasedOnAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await userService.GetByEmailAsync(loginDto.Email, false, cancellationToken)
            ?? throw new FlashTeamsException(401, "Login or password is incorrect!");

        return GenerateTokenBasedOn(user, loginDto);
    }

    public string? GetClaim(string claimName, bool throwExceptionIfNotFound = false)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(claimName)?.Value;

        if (value is null && throwExceptionIfNotFound)
        {
            throw new FlashTeamsException(401, "Claim not found");
        }

        return value;
    }
}
