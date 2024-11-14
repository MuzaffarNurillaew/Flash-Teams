using FlashTeams.Api.Configurations;

var builder = WebApplication
    .CreateBuilder(args)
    .Configure();

var app = await builder
    .Build()
    .ConfigureAsync();

await app.RunAsync();
