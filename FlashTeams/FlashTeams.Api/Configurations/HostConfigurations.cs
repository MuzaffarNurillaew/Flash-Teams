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

    public static WebApplication Configure(this WebApplication app)
    {
        app = app
            .ConfigureDevTools()
            .ConfigureControllers()
            .ConfigureMiddlewares()
            .ConfigureCors();

        return app;
    }
}