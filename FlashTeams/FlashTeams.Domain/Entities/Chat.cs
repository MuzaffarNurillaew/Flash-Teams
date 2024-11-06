using FlashTeams.Domain.Entities.Commons;
using FlashTeams.Domain.Entities.Enums;

namespace FlashTeams.Domain.Entities;

public class Chat : IFlashTeamsEntity, IEntity
{
    public Guid Id { get; set; }

    public ChatType Type { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}