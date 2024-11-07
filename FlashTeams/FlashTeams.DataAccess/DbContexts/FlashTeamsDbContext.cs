using FlashTeams.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlashTeams.DataAccess.DbContexts;

public class FlashTeamsDbContext(DbContextOptions<FlashTeamsDbContext> options)
    : DbContext(options)
{
    public DbSet<Chat> Chats => Set<Chat>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}