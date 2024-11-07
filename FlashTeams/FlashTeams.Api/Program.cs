using FlashTeams.Api.Configurations;

var builder = WebApplication
    .CreateBuilder(args)
    .Configure();

await builder
    .Build()
    .Configure()
    .RunAsync();
