using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.DataAccess.Repositories;
using FlashTeams.Domain.Entities;
using FluentValidation;

namespace FlashTeams.BusinessLogic.Services;

public class UserService(IRepository repository, IValidator<User> validator) : IUserService
{
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(user, cancellationToken: cancellationToken);

        user.PasswordHash = HashPassword(user.PasswordHash);

        return await repository.InsertAsync(user, cancellationToken: cancellationToken);
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return repository.DeleteAsync<User>(
            expression: user => user.Id == id,
            shouldThrowException: true,
            cancellationToken: cancellationToken);
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return repository.SelectAllAsync<User>(
            shouldTrack: false,
            cancellationToken: cancellationToken);
    }

    public Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return repository.SelectAsync<User>(
            expression: user => user.Id == id,
            shouldThrowException: true,
            shouldTrack: false,
            cancellationToken: cancellationToken);
    }

    public Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return repository.SelectAsync<User>(
            expression: user => user.Username == username,
            shouldThrowException: true,
            shouldTrack: false,
            cancellationToken: cancellationToken);
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(user, cancellationToken: cancellationToken);

        user.PasswordHash = HashPassword(user.PasswordHash);

        return await repository.UpdateAsync(
            expression: u => u.Id == user.Id,
            entity: user,
            includes: default,
            shouldSave: true,
            cancellationToken: cancellationToken);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
