using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.Domain.Entities;

public class Message : IFlashTeamsEntity, IEntity
{
    public Guid Id { get; set; }

    public string Content { get; set; }

    public Guid SenderId { get; set; }

    public User Sender { get; set; }

    public Guid ChatId { get; set; }

    public Chat Chat { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
