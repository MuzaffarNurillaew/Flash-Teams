using FlashTeams.Domain.Entities;

namespace FlashTeams.BusinessLogic.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}