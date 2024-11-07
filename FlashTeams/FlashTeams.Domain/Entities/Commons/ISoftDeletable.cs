namespace FlashTeams.Domain.Entities.Commons;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}