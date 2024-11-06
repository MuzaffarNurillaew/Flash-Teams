using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.Domain.Entities;

public class User : IFlashTeamsEntity, IEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Username { get; set; }

    public string PhoneNumber { get; set; }

    public string PasswordHash { get; set; }
}
