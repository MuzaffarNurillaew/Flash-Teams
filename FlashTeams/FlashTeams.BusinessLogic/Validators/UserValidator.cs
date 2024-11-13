using FlashTeams.DataAccess.Repositories;
using FlashTeams.Domain.Entities;
using FluentValidation;

namespace FlashTeams.BusinessLogic.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator(IRepository repository)
    {
        RuleFor(user => user.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(user => user.LastName)
            .MaximumLength(50);

        RuleFor(user => user.PhoneNumber)
            .MustAsync(async (phone, ct) => !await repository.ExistsAsync<User>(
                    u => u.PhoneNumber == phone,
                    shouldThrowException: false,
                    cancellationToken: ct))
            .When(user => user.PhoneNumber is not null);

        RuleFor(user => user.Username)
            .MustAsync(async (username, ct) => !await repository.ExistsAsync<User>(
                    u => u.Username == username,
                    shouldThrowException: false,
                    cancellationToken: ct))
            .When(user => user.Username is not null);

        RuleSet("Update", () =>
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (email, ct) => await repository.ExistsAsync<User>(
                        u => u.Email == email,
                        shouldThrowException: false,
                        cancellationToken: ct));

            RuleFor(user => user.Id)
                .MustAsync(async (id, ct) => await repository.ExistsAsync<User>(
                        u => u.Id == id,
                        shouldThrowException: false,
                        cancellationToken: ct));
        });

        RuleSet("Create", () =>
        {
            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (email, ct) => !await repository.ExistsAsync<User>(
                        u => u.Email == email,
                        shouldThrowException: false,
                        cancellationToken: ct));
        });
    }
}