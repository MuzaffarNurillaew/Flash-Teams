using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.Domain.Entities;
public class UserActivity : IFlashTeamsEntity
{
    public Guid UserId { get; set; }

    public DateTimeOffset LastSeenTime { get; set; }
}
