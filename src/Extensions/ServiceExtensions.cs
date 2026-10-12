namespace src.Extensions;

using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Data.Interceptors;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using src.Config;
using src.Data;
using src.ExceptionHandler;
using src.Modules.Auth.Services;
using src.Modules.Shared.CQRS;
using src.Modules.Shared.Services.Email;
using StackExchange.Redis;

public static class ServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            services.AddOpenApi();
            services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict
            );
            services.AddHttpLogging();
            services.AddProblemDetails(options =>
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions.TryAdd(
                        "requestId",
                        ctx.HttpContext.TraceIdentifier
                    );
                    ctx.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
                }
            );
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services
                .AddValidatorsFromAssembly(typeof(ServiceExtensions).Assembly)
                .AddConfig(configuration)
                .AddDatabase()
                .AddRedis()
                .AddServices()
                .AddScrutor()
                .ConfigureCors(configuration)
                .ConfigureRateLimiter(configuration);

            return services;
        }

        private IServiceCollection AddConfig(IConfiguration configuration)
        {
            services
                .AddOptionsWithValidateOnStart<Config>()
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

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<Config>>().Value);

            return services;
        }

        private IServiceCollection AddDatabase()
        {
            services.AddScoped<ISaveChangesInterceptor, DateInterceptor>();

            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    var cfg = sp.GetRequiredService<Config>();
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
                var config = sp.GetRequiredService<Config>();

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
            services.AddSingleton<SessionStore>();

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

        private IServiceCollection ConfigureCors(IConfiguration configuration)
        {
            services.AddCors(op =>
            {
                var applicationConfig = configuration
                    .GetRequiredSection("Application")
                    .Get<ApplicationConfig>();

                ArgumentNullException.ThrowIfNull(applicationConfig);

                op.AddDefaultPolicy(options =>
                {
                    options
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins(applicationConfig.ClientUrl)
                        .AllowCredentials();
                });
            });

            return services;
        }

        private IServiceCollection ConfigureRateLimiter(IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                var rateLimitConfig = configuration
                    .GetRequiredSection("RateLimit")
                    .Get<RateLimitConfig>();

                ArgumentNullException.ThrowIfNull(rateLimitConfig);

                foreach (var property in typeof(RateLimitConfig).GetProperties())
                {
                    var value = property.GetValue(rateLimitConfig, null);
                    if (value is null)
                        continue;

                    options.AddFixedWindowLimiter(
                        property.Name,
                        cfg =>
                        {
                            cfg.PermitLimit = (int)value;
                            cfg.Window = TimeSpan.FromMinutes(1);
                        }
                    );
                }

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (ctx, token) =>
                {
                    if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        ctx.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                        await ctx.HttpContext.Response.WriteAsJsonAsync(
                            new ProblemDetails
                            {
                                Status = StatusCodes.Status429TooManyRequests,
                                Title = "Too Many Requests",
                                Detail =
                                    $"Too many requests. Try again after {retryAfter.TotalSeconds} seconds.",
                                Extensions =
                                {
                                    ["requestId"] = ctx.HttpContext.TraceIdentifier,
                                    ["timestamp"] = DateTime.UtcNow,
                                },
                            },
                            token
                        );
                    }
                };
            });

            return services;
        }
    }
}
