using System.Text;
using Azure.Identity;
using FlashTeams.Api.Middlewares;
using FlashTeams.BusinessLogic.Interfaces;
using FlashTeams.BusinessLogic.Services;
using FlashTeams.BusinessLogic.Validators;
using FlashTeams.DataAccess.DbContexts;
using FlashTeams.DataAccess.Mappers;
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
    private static WebApplicationBuilder ConfigureCredentials(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            builder.Configuration.AddAzureKeyVault(
                vaultUri: new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
                credential: new DefaultAzureCredential());
        }

        return builder;
    }

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
        })
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration.GetValue<string>("Authentication:Google:ClientId") ?? string.Empty;
            options.ClientSecret = builder.Configuration.GetValue<string>("Authentication:Google:ClientSecret") ?? string.Empty;
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRepository, Repository>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddAutoMapper(typeof(EntityMapper).Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

        builder.Services.AddHttpContextAccessor();

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

        builder.Services.AddSingleton(serviceProvider =>
        {
            return new GoogleAuthSettings
            {
                ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty,
                ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty,
            };
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureDevTools(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Flash Teams", Version = "v1" });

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
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
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
        app.Use(async (context, next) =>
        {
            if (!context.Response.Headers.TryAdd("Cross-Origin-Opener-Policy", "same-origin"))
            {
                context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
            }

            await next();
        });

        return app;
    }

    private static WebApplication ConfigureCors(this WebApplication app)
    {
        app.UseCors(options =>
        {
            options.AllowAnyOrigin();
            options.AllowAnyMethod();
            options.AllowAnyHeader();
        });

        return app;
    }
}
