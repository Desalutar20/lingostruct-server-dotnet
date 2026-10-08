using FluentValidation;
using Microsoft.Extensions.Options;
using src.Config;
using src.Data;
using src.Modules.Auth;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Services.Email;
using StackExchange.Redis;

namespace src.Extensions;

using Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            services
                .AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly)
                .AddConfig(configuration)
                .AddDatabase()
                .AddRedis()
                .AddServices()
                .AddScrutor();

            return services;
        }

        private IServiceCollection AddConfig(IConfiguration configuration)
        {
            services
                .AddOptionsWithValidateOnStart<Config.Config>()
                .Bind(configuration)
                .Validate(x =>
                {
                    ConfigValidator validator = new();

                    var result = validator.Validate(x);
                    if (result.IsValid)
                    {
                        return true;
                    }

                    foreach (var error in result.Errors)
                        Console.WriteLine(
                            $"[CONFIG ERROR] Property: {error.PropertyName}, Error: {error.ErrorMessage}, Attempted Value: {error.AttemptedValue}"
                        );

                    return result.IsValid;
                });

            services.AddSingleton<Config.Config>(sp =>
                sp.GetRequiredService<IOptions<Config.Config>>().Value
            );

            return services;
        }

        private IServiceCollection AddDatabase()
        {
            services.AddScoped<ISaveChangesInterceptor, DateInterceptor>();

            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    var cfg = sp.GetRequiredService<Config.Config>();
                    options
                        .UseNpgsql(cfg.Database.GetConnectionString())
                        .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                        .UseSnakeCaseNamingConvention();
                }
            );

            return services;
        }

        private IServiceCollection AddRedis()
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = sp.GetRequiredService<Config.Config>();

                ConfigurationOptions options = new()
                {
                    EndPoints = { $"{config.Redis.Host}:{config.Redis.Port}" },
                    User = string.IsNullOrEmpty(config.Redis.User) ? null : config.Redis.User,
                    Password = config.Redis.Password,
                    DefaultDatabase = config.Redis.Database,
                };

                return ConnectionMultiplexer.Connect(options);
            });

            return services;
        }

        private IServiceCollection AddServices()
        {
            services.AddSingleton<EmailService>();
            services.AddSingleton<AuthCache>();

            return services;
        }

        private IServiceCollection AddScrutor()
        {
            services.Scan(scan =>
                scan.FromAssemblies(typeof(ServiceExtensions).Assembly)
                    .AddClasses(
                        classes =>
                            classes.AssignableToAny(
                                typeof(ICommandHandler<>),
                                typeof(ICommandHandler<,>)
                            ),
                        false
                    )
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), false)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }
    }
}
