using FlashTeams.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace FlashTeams.Test.Helpers;

internal class DatabaseUtilities : IDisposable, IAsyncDisposable
{
    private readonly FakeDataGenerators _fillers = new();

    public DatabaseUtilities()
    {
        var inMemoryOptions = new DbContextOptionsBuilder<FlashTeamsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        FlashTeamsContext = new FlashTeamsDbContext(inMemoryOptions);
        FlashTeamsContext.Database.EnsureCreated();
    }

    public FlashTeamsDbContext FlashTeamsContext { get; }

    public void Seed(int numberOfRecords = 10)
    {
        FlashTeamsContext.AddRange(_fillers.UserMinimalFiller.Create(numberOfRecords));
        FlashTeamsContext.SaveChanges();
    }

    public void Dispose()
    {
        FlashTeamsContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await FlashTeamsContext.DisposeAsync();
    }
}
