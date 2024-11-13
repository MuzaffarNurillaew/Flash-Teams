using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.DataAccess.Repositories;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Dtos.Auth;
using FlashTeams.Shared.Exceptions;
using FluentValidation;

namespace FlashTeams.BusinessLogic.Services;

public class UserService(IRepository repository, IValidator<User> validator) : IUserService
{
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAsync(user, opt => opt.IncludeRuleSets("Create").ThrowOnFailures(), cancellationToken);

        SetHashedPassword(user);

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

    public Task<User> GetByUsernameAsync(string username, bool shouldThrowException = true, CancellationToken cancellationToken = default)
    {
        return repository.SelectAsync<User>(
            expression: user => user.Username == username,
            shouldThrowException: shouldThrowException,
            shouldTrack: false,
            cancellationToken: cancellationToken);
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAsync(user, opt => opt.IncludeRuleSets("Update").ThrowOnFailures(), cancellationToken);

        SetHashedPassword(user);

        return await repository.UpdateAsync(
            expression: u => u.Id == user.Id,
            entity: user,
            includes: default,
            shouldSave: true,
            cancellationToken: cancellationToken);
    }

    public Task<User> GetByEmailAsync(string email, bool shouldThrowException = true, CancellationToken cancellationToken = default)
    {
        return repository.SelectAsync<User>(
            expression: user => user.Email == email,
            shouldThrowException: shouldThrowException,
            shouldTrack: false,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> SetPasswordFirstTimeAsync(string email, FirstTimePasswordCreationDto setPasswordFirstTimeDto, CancellationToken cancellationToken)
    {
        var user = await GetByEmailAsync(email!, true, cancellationToken);

        if (user.PasswordHash != null)
        {
            throw new FlashTeamsException(400, "Password is already set, use different endpoint to reset password.");
        }

        user.PasswordHash = setPasswordFirstTimeDto.Password;

        await UpdateAsync(user, cancellationToken);

        return true;
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static void SetHashedPassword(User user)
    {
        if (user.PasswordHash is not null)
        {
            user.PasswordHash = HashPassword(user.PasswordHash);
        }
    }
}
