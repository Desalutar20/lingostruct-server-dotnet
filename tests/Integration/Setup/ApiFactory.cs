using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using src.Config;
using src.Data;
using Xunit;

namespace Integration.Setup;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public override async ValueTask DisposeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.EnsureDeletedAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // builder.ConfigureLogging(logger => logger.ClearProviders());

        builder.ConfigureAppConfiguration(
            (_, configBuilder) =>
            {
                var guid = Guid.NewGuid();
                configBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Database:Name"] = $"test-{guid}",
                        ["Database:MaxPoolSize"] = "200",

                        ["Redis:KeyPrefix"] = $"{guid}:",

                        ["RateLimit:SignUp"] = "40",
                        ["RateLimit:SignIn"] = "40",
                        ["RateLimit:VerifyAccount"] = "40",
                        ["RateLimit:ForgotPassword"] = "20",
                        ["RateLimit:ResetPassword"] = "20",
                    }
                );
            }
        );

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(
                (sp, opt) =>
                {
                    var cfg = sp.GetRequiredService<Config>();
                    opt.UseNpgsql(cfg.Database.GetConnectionString())
                        .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                        .UseSnakeCaseNamingConvention();
                }
            );
        });

        base.ConfigureWebHost(builder);
    }
}
