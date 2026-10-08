using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Modules.Auth.Endpoints;

namespace src.Extensions;

public static class AppExtensions
{
    extension(WebApplication app)
    {
        public async Task ApplyMigrations()
        {
            await using var scope = app.Services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.MigrateAsync();
        }

        public WebApplication AddEndpoints()
        {
            var group = app.MapGroup("/api/v1");

            group.MapAuthV1();

            return app;
        }
    }
}
