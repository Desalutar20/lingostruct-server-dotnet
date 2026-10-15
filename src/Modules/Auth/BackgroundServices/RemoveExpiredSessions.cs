using src.Modules.Auth.UseCases.Commands.RemoveExpiredSessions;
using src.Modules.Shared.CQRS;

namespace src.Modules.Auth.BackgroundServices;

internal sealed class RemoveExpiredSessions(
    ILogger<RemoveExpiredSessions> logger,
    IServiceScopeFactory serviceFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(10));

        do
        {
            try
            {
                await using var scope = serviceFactory.CreateAsyncScope();
                var commandHandler = scope.ServiceProvider.GetRequiredService<
                    ICommandHandler<RemoveExpiredSessionCommand>
                >();

                var result = await commandHandler.Handle(
                    new RemoveExpiredSessionCommand(),
                    stoppingToken
                );
                if (result.IsFailure)
                {
                    logger.LogError("Failed to remove expired sessions: {Error}", result.Error);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while removing expired sessions");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
