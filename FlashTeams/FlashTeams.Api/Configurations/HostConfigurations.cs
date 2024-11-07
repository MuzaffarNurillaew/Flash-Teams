namespace FlashTeams.Api.Configurations;

public static partial class HostConfigurations
{
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder = builder
            .ConfigureStorage()
            .ConfigureServices()
            .ConfigureDevTools()
            .ConfigureControllers();

        return builder;
    }

    public static WebApplication Configure(this WebApplication app)
    {
        app = app
            .ConfigureDevTools()
            .ConfigureControllers()
            .ConfigureMiddlewares();

        return app;
    }
}