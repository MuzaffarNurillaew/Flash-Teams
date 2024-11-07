using FlashTeams.Api.Middlewares;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.BusinessLogic.Services;
using FlashTeams.BusinessLogic.Validators;
using FlashTeams.DataAccess.DbContexts;
using FlashTeams.DataAccess.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlashTeams.Api.Configurations;

public static partial class HostConfigurations
{
    public static WebApplicationBuilder ConfigureStorage(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<FlashTeamsDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        return builder;
    }

    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRepository, Repository>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddAutoMapper(typeof(UserService).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

        return builder;
    }

    public static WebApplicationBuilder ConfigureDevTools(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();

        return builder;
    }

    public static WebApplicationBuilder ConfigureControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        return builder;
    }

    public static WebApplication ConfigureDevTools(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication ConfigureControllers(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static WebApplication ConfigureMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}
