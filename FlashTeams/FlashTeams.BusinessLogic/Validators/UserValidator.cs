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
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) => !await repository.ExistsAsync<User>(
                    u => u.Email == email,
                    shouldThrowException: false,
                    cancellationToken: ct));

        RuleFor(user => user.PhoneNumber)
            .NotEmpty()
            .MustAsync(async (phone, ct) => !await repository.ExistsAsync<User>(
                    u => u.PhoneNumber == phone,
                    shouldThrowException: false,
                    cancellationToken: ct));
    }
}