using GjejVend.Application.Bookings;

namespace GjejVend.Api.Workers;

internal sealed class BookingLifecycleWorker(IServiceScopeFactory scopes, ILogger<BookingLifecycleWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var closed = await scope.ServiceProvider.GetRequiredService<BookingLifecycleService>().CloseOverdueAsync(stoppingToken);
                if (closed > 0)
                {
                    logger.LogInformation("Closed {Count} overdue bookings", closed);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Closing overdue bookings failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
