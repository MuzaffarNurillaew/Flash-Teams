using System.Linq.Expressions;
using AutoMapper;
using FlashTeams.DataAccess.Repositories;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Exceptions;
using FlashTeams.Test.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

#pragma warning disable CA1707
#pragma warning disable SA1010
namespace FlashTeams.Test.DataAccessLayer;

public class RepositoryTests : IDisposable
{
    private readonly Repository _repository;
    private readonly DatabaseUtilities _databaseUtilities = new();
    private readonly FakeDataGenerators _filler = new();
    private readonly Mock<IMapper> _mapperMock;

    public RepositoryTests()
    {
        _databaseUtilities.Seed();
        _mapperMock = new Mock<IMapper>();
        _repository = new Repository(_databaseUtilities.FlashTeamsContext, _mapperMock.Object);
    }

    [Fact]
    public async Task InsertAsync_Should_InsertEntity_WhenSaved()
    {
        // Arrange
        var user = _filler.UserMinimalFiller.Create();

        // Act
        var insertedUser = await _repository.InsertAsync(user);
        var lastInsertedUser = await _databaseUtilities.FlashTeamsContext.Users.FirstOrDefaultAsync(g => g.Id == insertedUser.Id);

        // Assert
        lastInsertedUser.Should().BeEquivalentTo(insertedUser);
    }

    [Fact]
    public async Task InsertAsync_Should_NotSaveOnlyTrackEntity_WhenNotSaved()
    {
        // Arrange
        var lastInsertedUserBefore = await _databaseUtilities.FlashTeamsContext.Users.LastOrDefaultAsync();
        var user = _filler.UserMinimalFiller.Create();

        // Act
        var insertedUser = await _repository.InsertAsync(user, false);
        var lastInsertedUserAfter = await _databaseUtilities.FlashTeamsContext.Users.LastOrDefaultAsync();

        // Assert
        lastInsertedUserBefore.Should().BeEquivalentTo(lastInsertedUserAfter);
        insertedUser.Should().NotBeEquivalentTo(lastInsertedUserAfter);
    }

    [Fact]
    public async Task InsertManyAsync_Should_CreateAllEntities()
    {
        // Arrange
        int usersCount = 3;
        var users = _filler.UserMinimalFiller.Create(usersCount);
        int totalNumberOfUsersBeforeInsertion = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        await _repository.InsertManyAsync(users);
        var lastInsertedUsers = _databaseUtilities.FlashTeamsContext.Users.Skip(totalNumberOfUsersBeforeInsertion);

        // Assert
        lastInsertedUsers.Should().NotBeNullOrEmpty();
        (await lastInsertedUsers.CountAsync()).Should().Be(usersCount);
        (await _databaseUtilities.FlashTeamsContext.Users.CountAsync()).Should().Be(totalNumberOfUsersBeforeInsertion + usersCount);
    }

    [Fact]
    public async Task InsertManyAsync_Should_NotSaveOnlyTrackEntity_WhenNotSaved()
    {
        // Arrange
        int usersCount = 5;
        var users = _filler.UserMinimalFiller.Create(usersCount);
        int totalNumberOfUsersBeforeInsertion = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        await _repository.InsertManyAsync(users, false);
        var lastInsertedUsers = _databaseUtilities.FlashTeamsContext.Users.Skip(totalNumberOfUsersBeforeInsertion);

        // Assert
        lastInsertedUsers.Should().BeNullOrEmpty();
        (await _databaseUtilities.FlashTeamsContext.Users.CountAsync()).Should().Be(totalNumberOfUsersBeforeInsertion);
    }

    [Fact]
    public async Task SelectAsync_Should_ThrowException_WhenNoEntityFound()
    {
        // Arrange
        Expression<Func<User, bool>> expression = user => false;

        // Act
        var task = async () => await _repository.SelectAsync(expression, true);

        // Assert
        await task.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task SelectAsync_Should_SilentlyReturnNull_WhenNoEntityFound()
    {
        // Arrange
        Expression<Func<User, bool>> expression = user => false;

        // Act
        var result = await _repository.SelectAsync(expression);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SelectAsync_Should_ReturnFoundEntity()
    {
        // Arrange
        await _databaseUtilities.FlashTeamsContext.Users.AddRangeAsync(_filler.UserMinimalFiller.Create(100));
        await _databaseUtilities.FlashTeamsContext.SaveChangesAsync();

        var firstEntity = await _databaseUtilities.FlashTeamsContext.Users.AsNoTracking().FirstOrDefaultAsync();
        var username = firstEntity.Username;

        // Act
        var result = await _repository.SelectAsync<User>(user => user.Username == username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(firstEntity);
    }

    [Fact]
    public void SelectAll_Should_ReturnEmptyQuery()
    {
        // Act
        var usersQuery =
            _repository.SelectAll<User>(expression: user => false, shouldTrack: false);

        // Assert
        usersQuery.Count().Should().Be(0);
    }

    [Fact]
    public async Task SelectAll_Should_ReturnFilteredQuery()
    {
        // Arrange
        var totalUsersCount = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        var usersQuery =
            _repository.SelectAll<User>(expression: user => true, shouldTrack: true);

        // Assert
        (await usersQuery.CountAsync()).Should().Be(totalUsersCount);

        // tracking
        var firstItem = await usersQuery.FirstOrDefaultAsync();
        var isTracked = _databaseUtilities.FlashTeamsContext.ChangeTracker.Entries<User>().Any(x => x.Entity.Id == firstItem.Id);
        isTracked.Should().BeTrue();
    }

    [Fact]
    public async Task SelectAll_Should_ReturnFilteredQueriesWithoutTracking()
    {
        // Arrange
        var totalUsersCount = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        var usersQuery = _repository.SelectAll<User>(
            expression: user => true,
            shouldTrack: false);

        // Assert
        (await usersQuery.CountAsync()).Should().Be(totalUsersCount);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateFoundEntity()
    {
        // Arrange
        var firstEntity = await _repository.SelectAsync<User>(
            expression: user => true,
            shouldThrowException: true,
            shouldTrack: false);

        var newEntity = _filler.UserMinimalFiller.Create();
        newEntity.Id = firstEntity.Id;
        ConfigureMapperMock();

        // Act
        // TODO: change the usage
        var updatedEntity = await _repository.UpdateAsync(
            expression: user => user.Id == newEntity.Id,
            entity: newEntity,
            includes: default,
            shouldSave: true);

        // Assert
        updatedEntity.Should().BeEquivalentTo(newEntity);
        updatedEntity.Should().NotBeEquivalentTo(firstEntity);
    }

    [Fact]
    public async Task UpdateAsync_Should_ThrowExceptionIfExpressionDoesNotMatch()
    {
        // Arrange
        var newEntity = _filler.UserMinimalFiller.Create();

        // Act
        // TODO: change the usage
        var updateFunc = async () => await _repository.UpdateAsync(
            expression: user => false,
            entity: newEntity,
            includes: default,
            shouldSave: true);

        // Assert
        await updateFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task UpdateAsync_Should_NotSave()
    {
        // Arrange
        var firstEntity = await _repository.SelectAsync<User>(
            expression: user => true,
            shouldThrowException: true,
            shouldTrack: false);

        var newEntity = _filler.UserMinimalFiller.Create();
        newEntity.Id = firstEntity.Id;
        newEntity.PhoneNumber = firstEntity.PhoneNumber;
        ConfigureMapperMock();

        // Act
        var updatedEntity = await _repository.UpdateAsync(
            expression: user => user.Id == newEntity.Id,
            includes: default,
            entity: newEntity,
            shouldSave: false);

        // Assert
        var firstEntityAfterUpdate = await _repository.SelectAsync<User>(
            expression: user => user.Id == firstEntity.Id,
            shouldThrowException: true,
            shouldTrack: false);

        updatedEntity.Should().NotBeEquivalentTo(firstEntityAfterUpdate);
        updatedEntity.Should().NotBeEquivalentTo(firstEntity);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalseIfNotExists()
    {
        // Act
        var result = await _repository.ExistsAsync<User>(
            expression: user => false,
            shouldThrowException: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_ThrowNotFoundExceptionIfNotExists()
    {
        // Act
        var existsFunc = async () => await _repository.ExistsAsync<User>(
            expression: user => false,
            shouldThrowException: true);

        // Assert
        await existsFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrueIfExists()
    {
        // Act
        var result = await _repository.ExistsAsync<User>(
            expression: user => true,
            shouldThrowException: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowNotFoundException_WhenExpressionDoesNotMatch()
    {
        // Act
        var deleteFunc = async () => await _repository.DeleteAsync<User>(
            expression: user => false,
            shouldSave: true,
            shouldThrowException: true);

        // Assert
        await deleteFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenExpressionDoesNotMatch()
    {
        // Act
        var isDeleted = await _repository.DeleteAsync<User>(
            expression: user => false,
            shouldSave: true,
            shouldThrowException: false);

        // Assert
        isDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnTrue_WhenEntityIsSoftDeleted()
    {
        // Arrange
        var fakeUser = _filler.UserMinimalFiller.Create();
        fakeUser.FirstName = "DeleteAsync_FakeName";
        var numberOfRecordsInitially = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();
        await _databaseUtilities.FlashTeamsContext.Users.AddAsync(fakeUser);
        await _databaseUtilities.FlashTeamsContext.SaveChangesAsync();
        var numberOfRecordAfterInsert = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        var isDeleted = await _repository.DeleteAsync<User>(
            expression: user => user.FirstName == fakeUser.FirstName,
            shouldSave: true,
            shouldThrowException: true);
        var numberOfRecordAfterDelete = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Assert
        isDeleted.Should().BeTrue();
        numberOfRecordAfterInsert.Should().Be(numberOfRecordsInitially + 1);
        numberOfRecordsInitially.Should().Be(numberOfRecordAfterDelete);
        (await _databaseUtilities.FlashTeamsContext.Users.AnyAsync(user => user.FirstName == fakeUser.FirstName)).Should()
            .BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnTrue_WhenEntityIsDeleted()
    {
        // Arrange
        var fakeUser = _filler.UserMinimalFiller.Create();
        fakeUser.FirstName = "DeleteAsync_FakeName";
        var numberOfRecordsInitially = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();
        await _databaseUtilities.FlashTeamsContext.Users.AddAsync(fakeUser);
        await _databaseUtilities.FlashTeamsContext.SaveChangesAsync();
        var numberOfRecordAfterInsert = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Act
        var isDeleted = await _repository.DeleteAsync<User>(
            expression: user => user.FirstName == fakeUser.FirstName,
            shouldSave: true,
            shouldThrowException: true);
        var numberOfRecordAfterDelete = await _databaseUtilities.FlashTeamsContext.Users.CountAsync();

        // Assert
        isDeleted.Should().BeTrue();
        numberOfRecordAfterInsert.Should().Be(numberOfRecordsInitially + 1);
        numberOfRecordsInitially.Should().Be(numberOfRecordAfterDelete);
        (await _databaseUtilities.FlashTeamsContext.Users.AnyAsync(user => user.FirstName == fakeUser.FirstName)).Should()
            .BeFalse();
    }

    [Fact]
    public async Task SelectAllAsync_ShouldReturnListOfFoundEntities()
    {
        // Arrange
        var users = await _databaseUtilities.FlashTeamsContext.Users.ToListAsync();

        // Act
        var foundUsers = await _repository.SelectAllAsync<User>();

        // Assert
        foundUsers.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetTotalCountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var numberOfRecords = await _databaseUtilities.FlashTeamsContext.Set<User>().CountAsync(_ => true);

        // Act
        var totalCount = await _repository.GetTotalCountAsync<User>(_ => true);

        // Assert
        totalCount.Should().Be(numberOfRecords);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private void ConfigureMapperMock()
    {
        _mapperMock.Setup(mapper => mapper.Map(It.IsAny<User>(), It.IsAny<User>()))
            .Callback((User source, User destination) =>
            {
                destination.Id = source.Id;
                destination.Email = source.Email;
                destination.FirstName = source.FirstName;
                destination.LastName = source.LastName;
                destination.Username = source.Username;
                destination.PhoneNumber = source.PhoneNumber;
                destination.PasswordHash = source.PasswordHash;
                destination.GoogleId = source.GoogleId;
            });
    }
}