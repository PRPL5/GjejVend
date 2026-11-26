using Microsoft.Extensions.Logging;

namespace GjejVend.Infrastructure.Bookings;

public interface INotificationSender
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
}

internal sealed partial class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger) : INotificationSender
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        LogUndelivered(logger, subject);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "No email provider is configured, so the email \"{Subject}\" was not delivered")]
    private static partial void LogUndelivered(ILogger logger, string subject);
}
