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

    // create Configure(WebApplication)
    public static WebApplication Configure(this WebApplication app)
    {
        app
            .ConfigureDevTools()
            .ConfigureControllers();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}