using FlashTeams.Domain.Entities;

namespace FlashTeams.Shared.Dtos.Users;

public class UserResultDto(User createdUser)
{
    public Guid Id { get; set; } = createdUser.Id;

    public string FirstName { get; set; } = createdUser.FirstName;

    public string? LastName { get; set; } = createdUser.LastName;

    public string Email { get; set; } = createdUser.Email;

    public string? PhoneNumber { get; set; } = createdUser.PhoneNumber;

    public string Username { get; set; } = createdUser.Username;

    public static UserResultDto Create(User user)
    {
        return new UserResultDto(user);
    }
}