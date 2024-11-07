using System.Linq.Expressions;
using FlashTeams.Domain.Entities.Commons;

namespace FlashTeams.DataAccess.Repositories;

public interface IRepository
{
    Task<T> InsertAsync<T>(
        T entity,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task InsertManyAsync<T>(
        IEnumerable<T> entities,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task<T> SelectAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        bool shouldTrack = true,
        string[]? includes = null,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    IQueryable<T> SelectAll<T>(
        Expression<Func<T, bool>>? expression = null,
        bool shouldTrack = true,
        string[]? includes = null)
        where T : class, IFlashTeamsEntity;

    Task<List<T>> SelectAllAsync<T>(
        Expression<Func<T, bool>>? expression = null,
        bool shouldTrack = true,
        string[]? includes = null,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task<T> UpdateAsync<T>(
        Expression<Func<T, bool>> expression,
        T entity,
        string[]? includes,
        bool shouldThrowException = true,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task<bool> DeleteAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task DeleteManyAsync<T>(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task<bool> ExistsAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity;
}