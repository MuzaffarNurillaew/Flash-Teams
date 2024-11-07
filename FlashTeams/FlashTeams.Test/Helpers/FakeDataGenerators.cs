using FlashTeams.Domain.Entities;
using Tynamix.ObjectFiller;

namespace FlashTeams.Test.Helpers;

internal class FakeDataGenerators
{
    public FakeDataGenerators()
    {
    }

    public Filler<User> UserMinimalFiller { get; } = new();
}