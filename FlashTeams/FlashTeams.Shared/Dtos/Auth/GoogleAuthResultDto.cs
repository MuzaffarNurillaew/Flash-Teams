namespace FlashTeams.Shared.Dtos.Auth;

public class GoogleAuthResultDto(string token) : AuthResultDto(token)
{
    public bool IsNewUser { get; set; }

    public string Email { get; set; }
}