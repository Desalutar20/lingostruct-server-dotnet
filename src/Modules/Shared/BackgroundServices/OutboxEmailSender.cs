using System.Text.Json;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Modules.Shared.Domain.ValueObjects;
using src.Modules.Shared.Outbox;
using src.Modules.Shared.Services.Email;

namespace src.Modules.Shared.BackgroundServices;

internal sealed class OutboxEmailSender(
    ILogger<OutboxEmailSender> logger,
    IServiceScopeFactory serviceFactory
) : BackgroundService
{
    private const int BatchSize = 100;
    private const int MaxAttempts = 3;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new EmailMessageConverter() },
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(20));

        do
        {
            await using var scope = serviceFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                stoppingToken
            );

            try
            {
                var outboxes = await dbContext.Outboxes.GetAndLockOutboxesForProcessingAsync(
                    OutboxType.Email,
                    PositiveInt.Create(BatchSize).Value,
                    stoppingToken
                );

                var processedIds = new List<OutboxId>(BatchSize);

                foreach (var outbox in outboxes)
                    try
                    {
                        var success = await TrySendWithRetry(emailService, outbox, stoppingToken);
                        if (success)
                            processedIds.Add(outbox.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Unexpected error while processing Outbox {OutboxId}",
                            outbox.Id
                        );
                    }

                if (processedIds.Count > 0)
                {
                    var now = DateTimeOffset.UtcNow;

                    await dbContext.Outboxes.MarkAsProcessedAsync(processedIds, now, stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while processing email outbox");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<bool> TrySendWithRetry(
        EmailService emailSender,
        Outbox.Outbox outbox,
        CancellationToken ct
    )
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                var message = JsonSerializer.Deserialize<EmailMessage>(outbox.Data.Value, Options);
                var result = await emailSender.SendAsync(message, ct);

                if (result.IsSuccess)
                    return true;

                logger.LogWarning(
                    "Failed to send email for Outbox {OutboxId}: {Error}",
                    outbox.Id,
                    result.Error.Description
                );

                return false;
            }
            catch (SmtpCommandException ex) when ((int)ex.StatusCode is >= 400 and < 500)
            {
                logger.LogWarning(ex, "Temporary SMTP error on attempt {Attempt}", attempt);
            }
            catch (SmtpProtocolException ex)
            {
                logger.LogWarning(ex, "SMTP protocol error on attempt {Attempt}", attempt);
            }
            catch (IOException ex)
            {
                logger.LogWarning(ex, "Network error on attempt {Attempt}", attempt);
            }

            if (attempt == MaxAttempts)
            {
                logger.LogWarning(
                    "Failed to send email after retries for Outbox {OutboxId}",
                    outbox.Id
                );
                return false;
            }

            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), ct);
        }

        return false;
    }
}
