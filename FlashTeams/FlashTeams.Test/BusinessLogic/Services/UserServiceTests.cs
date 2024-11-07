using System.Linq.Expressions;
using FlashTeams.BusinessLogic.Services;
using FlashTeams.DataAccess.Repositories;
using FlashTeams.Domain.Entities;
using FlashTeams.Shared.Exceptions;
using FlashTeams.Test.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ValidationException = FluentValidation.ValidationException;

namespace FlashTeams.Test.BusinessLogic.Services;

#pragma warning disable CA1707 // Identifiers should not contain underscores
public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<IValidator<User>> _userValidatorMock;
    private readonly FakeDataGenerators _fillers = new();

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _userValidatorMock = new Mock<IValidator<User>>();

        _userService = new UserService(_repositoryMock.Object, _userValidatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException()
    {
        // Arrange
        var invalidUser = _fillers.UserMinimalFiller.Create();

        _userValidatorMock.Setup(validator =>
            validator.ValidateAsync(It.IsAny<ValidationContext<User>>(), default))
            .Throws(() => new ValidationException("Validation failed"));

        // Act
        var createFunc = async () => await _userService.CreateAsync(invalidUser);

        // Assert
        await createFunc.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewUser()
    {
        // Arrange
        var validUser = _fillers.UserMinimalFiller.Create();

        _userValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<ValidationContext<User>>(), default))
            .ReturnsAsync(new ValidationResult());

        _repositoryMock.Setup(repo =>
                repo.InsertAsync(validUser, true, default))
            .ReturnsAsync(validUser);

        // Act
        var createdPlatform = await _userService.CreateAsync(validUser);

        // Assert
        createdPlatform.Should().BeEquivalentTo(validUser);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(repo =>
                repo.DeleteAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    true,
                    default))
            .Throws<NotFoundException<User>>();

        // Act
        var deleteFunc = async () => await _userService.DeleteByIdAsync(Guid.NewGuid());

        // Assert
        await deleteFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFoundUser()
    {
        // Arrange
        _repositoryMock.Setup(repo =>
                repo.DeleteAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    true,
                    default))
            .ReturnsAsync(true);

        // Act
        var isDeleted = await _userService.DeleteByIdAsync(Guid.NewGuid());

        // Assert
        isDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFoundUser()
    {
        // Arrange
        var users = new List<User>();
        _repositoryMock.Setup(repo =>
                repo.SelectAllAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    false,
                    It.IsAny<string[]>(),
                    default))
            .ReturnsAsync(users);

        // Act
        var foundPlatforms = await _userService.GetAllAsync();

        // Assert
        foundPlatforms.Count.Should().Be(users.Count);
        foundPlatforms.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(repo =>
                repo.SelectAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    false,
                    default,
                    default))
            .Throws<NotFoundException<User>>();

        // Act
        var getFunc = async () => await _userService.GetByIdAsync(Guid.NewGuid());

        // Arrange
        await getFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFoundUser()
    {
        // Arrange
        var user = _fillers.UserMinimalFiller.Create();
        _repositoryMock.Setup(repo =>
                repo.SelectAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    false,
                    default,
                    default))
            .ReturnsAsync(user);

        // Act
        var foundUser = await _userService.GetByIdAsync(Guid.NewGuid());

        // Arrange
        foundUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(repo =>
                repo.SelectAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    false,
                    default,
                    default))
            .Throws<NotFoundException<User>>();

        // Act
        var getFunc = async () => await _userService.GetByUsernameAsync("019a7baa-8821-452d-b4df-802f8df2d460");

        // Arrange
        await getFunc.Should().ThrowAsync<NotFoundException<User>>();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnFoundUser()
    {
        // Arrange
        var user = _fillers.UserMinimalFiller.Create();
        _repositoryMock.Setup(repo =>
                repo.SelectAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    true,
                    false,
                    default,
                    default))
            .ReturnsAsync(user);

        // Act
        var foundUser = await _userService.GetByUsernameAsync("b763f529-67b1-403b-bf27-e2cf16d16d62");

        // Arrange
        foundUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowValidationException()
    {
        // Arrange
        var invalidUser = _fillers.UserMinimalFiller.Create();

        _userValidatorMock.Setup(validator =>
                validator.ValidateAsync(It.IsAny<ValidationContext<User>>(), default))
            .Throws(() => new ValidationException("Validation failed."));

        // Act
        var updateFunc = async () => await _userService.UpdateAsync(invalidUser);

        // Assert
        await updateFunc.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGenre()
    {
        // Arrange
        var validUser = _fillers.UserMinimalFiller.Create();

        _userValidatorMock.Setup(validator =>
                validator.ValidateAsync(It.IsAny<ValidationContext<User>>(), default))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>()));

        _repositoryMock.Setup(repo =>
                repo.UpdateAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    validUser,
                    It.IsAny<string[]>(),
                    true,
                    true,
                    default))
            .ReturnsAsync(validUser);

        // Act
        var updatedUser = await _userService.UpdateAsync(validUser);

        // Assert
        updatedUser.Should().BeEquivalentTo(validUser);
    }
}