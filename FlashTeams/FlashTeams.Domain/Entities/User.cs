using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.Domain.Entities;

public class User(
    string firstName,
    string lastName,
    string email,
    string username,
    string phoneNumber,
    string passwordHash) : IFlashTeamsEntity, IEntity
{
    public User(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string username,
        string phoneNumber,
        string password)
        : this(firstName, lastName, email, username, phoneNumber, password)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;
        PhoneNumber = phoneNumber;
        PasswordHash = password;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = firstName;

    public string LastName { get; set; } = lastName;

    public string Email { get; set; } = email;

    public string Username { get; set; } = username;

    public string PhoneNumber { get; set; } = phoneNumber;

    public string PasswordHash { get; set; } = passwordHash;
}
