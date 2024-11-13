using System.Text;
using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.Domain.Entities;

#pragma warning disable IDE0290
#pragma warning disable IDE0059
public class User : IFlashTeamsEntity, IEntity
{
    private string _userName;

    public User(Guid id, string firstName, string lastName, string email, string username, string phoneNumber, string password, string? googleId = default)
        : this(firstName, lastName, email, username, phoneNumber, password, googleId)
    {
        Id = id;
    }

    public User(string firstName, string lastName, string email, string username, string phoneNumber, string passwordHash, string? googleId = default)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Username = username;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
        GoogleId = googleId;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; }

    public string Username
    {
        get => _userName;
        set => _userName = value ?? GenerateUserName();
    }

    public string? GoogleId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    private string GenerateUserName()
    {
        var sb = new StringBuilder(FirstName);

        if (!string.IsNullOrWhiteSpace(LastName))
        {
            sb.Append($"-{LastName}");
        }

        sb.Append($"-{Id}");

        return sb.ToString();
    }
}
