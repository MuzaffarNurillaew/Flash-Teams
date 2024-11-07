namespace FlashTeams.Shared.Dtos.Auth;

public class AuthResultDto(string token)
{
    public string Token { get; set; } = token;
}