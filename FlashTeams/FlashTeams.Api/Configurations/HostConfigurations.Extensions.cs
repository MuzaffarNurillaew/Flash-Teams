using System.Text;
using FlashTeams.Api.Middlewares;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.BusinessLogic.Services;
using FlashTeams.BusinessLogic.Validators;
using FlashTeams.DataAccess.DbContexts;
using FlashTeams.DataAccess.Repositories;
using FlashTeams.Shared.Configurations;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FlashTeams.Api.Configurations;

public static partial class HostConfigurations
{
    private static WebApplicationBuilder ConfigureStorage(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<FlashTeamsDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = false,
                ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("JwtSettings:Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SecretKey")!)),
            };
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRepository, Repository>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddAutoMapper(typeof(UserService).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

        return builder;
    }

    private static WebApplicationBuilder RegisterSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(serviceProvider =>
        {
            return new JwtSettings
            {
                Audience = builder.Configuration.GetValue<string>("JwtSettings:Audience")!,
                Issuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer")!,
                SecretKey = builder.Configuration.GetValue<string>("JwtSettings:SecretKey")!,
                ExpirationMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpirationMinutes")!,
            };
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureDevTools(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "GameStore", Version = "v1" });

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "JWT Token, Do not include 'Bearer ' prefix.",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme,
                },
            };

            options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    jwtSecurityScheme,
                    Array.Empty<string>()
                },
            });
        });
        builder.Services.AddEndpointsApiExplorer();

        return builder;
    }

    private static WebApplicationBuilder ConfigureControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        return builder;
    }

    private static WebApplication ConfigureDevTools(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    private static WebApplication ConfigureControllers(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    private static WebApplication ConfigureMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}
