using System.Diagnostics;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Pocket.Data;

namespace Pocket.Application.Demo;

public class DemoCleanupService(
    IHostEnvironment hostEnvironment,
    IClock clock,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<DemoCleanupService> logger
) : BackgroundService
{
    private static readonly CronExpression Interval = CronExpression.Daily;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!hostEnvironment.IsDemo())
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = clock.GetCurrentInstant().ToDateTimeUtc();
            var nextOccurence = Interval.GetNextOccurrence(now, inclusive: true);
            Debug.Assert(nextOccurence.HasValue);
            var timeToWait = nextOccurence.Value - now;
            if (timeToWait > TimeSpan.Zero)
            {
                await Task.Delay(timeToWait, stoppingToken);
            }

            try
            {
                await RunCleanup(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Cleanup operation was canceled.");
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while running cleanup for demo environment");
            }
        }
    }

    private async Task RunCleanup(CancellationToken stoppingToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.SecureNotes.ExecuteDeleteAsync(stoppingToken);
    }
}