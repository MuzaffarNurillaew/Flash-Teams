using System.Linq.Expressions;
using AutoMapper;
using FlashTeams.DataAccess.DbContexts;
using FlashTeams.Domain.Entities.Commons;
using FlashTeams.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0046

namespace FlashTeams.DataAccess.Repositories;

public class Repository(FlashTeamsDbContext context, IMapper mapper) : IRepository
{
    private readonly DbContext _dbContext = context;

    public async Task<T> InsertAsync<T>(
        T entity,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        var insertedEntity = await set.AddAsync(entity, cancellationToken);

        if (shouldSave)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return insertedEntity.Entity;
    }

    public async Task InsertManyAsync<T>(
        IEnumerable<T> entities,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        await set.AddRangeAsync(entities, cancellationToken);
        if (shouldSave)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<T> SelectAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        bool shouldTrack = true,
        string[]? includes = null,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        var entityQuery = set.Where(expression);

        // throws exception if not found
        if (shouldThrowException && !await entityQuery.AnyAsync(cancellationToken))
        {
            throw new NotFoundException<T>();
        }

        entityQuery = shouldTrack ? entityQuery.AsTracking() : entityQuery.AsNoTracking();
        if (includes is not null && includes.Length != 0)
        {
            foreach (var include in includes)
            {
                entityQuery = entityQuery.Include(include);
            }
        }

        return await entityQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<T>> SelectAllAsync<T>(
        Expression<Func<T, bool>>? expression = null,
        bool shouldTrack = true,
        string[]? includes = null,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        return SelectAll(expression, shouldTrack, includes).ToListAsync(cancellationToken);
    }

    public IQueryable<T> SelectAll<T>(
        Expression<Func<T, bool>>? expression = null, bool shouldTrack = true, string[]? includes = null)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        var entityQuery = expression is not null ?
            set.Where(expression) :
            set.AsQueryable();

        if (includes is not null && includes.Length != 0)
        {
            foreach (var include in includes)
            {
                entityQuery = entityQuery.Include(include);
            }
        }

        if (!shouldTrack)
        {
            entityQuery.AsNoTracking();
        }

        return entityQuery;
    }

    public async Task<T> UpdateAsync<T>(
        Expression<Func<T, bool>> expression,
        T entity,
        string[]? includes,
        bool shouldThrowException = true,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var entityToUpdate = await SelectAsync(
            expression,
            shouldThrowException: shouldThrowException,
            shouldTrack: true,
            includes: includes,
            cancellationToken: cancellationToken);

        mapper.Map(entity, entityToUpdate);

        if (shouldSave)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return entityToUpdate;
    }

    public async Task DeleteManyAsync<T>(
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        var entitiesToDestroy = set.Where(expression);

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            foreach (var entity in entitiesToDestroy)
            {
                await DeleteAsync(
                    entity: entity,
                    shouldSave: true,
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            await entitiesToDestroy.ExecuteDeleteAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();

        var isExist = await set.AsNoTracking().AnyAsync(expression, cancellationToken);

        return shouldThrowException && !isExist ?
            throw new NotFoundException<T>() :
            isExist;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync<T>(
        Expression<Func<T, bool>> expression,
        bool shouldThrowException = false,
        bool shouldSave = true,
        CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        var set = _dbContext.Set<T>();
        var entityToDelete = await set.FirstOrDefaultAsync(expression, cancellationToken);

        if (entityToDelete is null)
        {
            return shouldThrowException switch
            {
                true => throw new NotFoundException<T>(),
                _ => false,
            };
        }

        return await DeleteAsync(entity: entityToDelete, shouldSave: true, cancellationToken: cancellationToken);
    }

    public Task<int> GetTotalCountAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        where T : class, IFlashTeamsEntity
    {
        return _dbContext.Set<T>().CountAsync(expression, cancellationToken);
    }

    private async Task<bool> DeleteAsync<T>(T entity, bool shouldSave, CancellationToken cancellationToken)
        where T : class, IFlashTeamsEntity
    {
        if (entity is ISoftDeletable deletable)
        {
            deletable.IsDeleted = true;
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        else
        {
            _dbContext.Remove(entity);
        }

        if (shouldSave)
        {
            await SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}