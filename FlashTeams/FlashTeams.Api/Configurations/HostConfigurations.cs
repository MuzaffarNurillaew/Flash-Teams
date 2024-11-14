namespace FlashTeams.Api.Configurations;

public static partial class HostConfigurations
{
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder = builder
            .ConfigureCredentials()
            .ConfigureStorage()
            .ConfigureServices()
            .ConfigureDevTools()
            .ConfigureControllers()
            .ConfigureAuthentication()
            .RegisterSettings();

        return builder;
    }

    public static async Task<WebApplication> ConfigureAsync(this WebApplication app)
    {
        await app.MigrateDatabaseSchemasAsync();

        app
            .ConfigureDevTools()
            .ConfigureControllers()
            .ConfigureMiddlewares()
            .ConfigureCors();

        return app;
    }
}