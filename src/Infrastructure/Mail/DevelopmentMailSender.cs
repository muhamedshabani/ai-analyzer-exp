using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Mail;

public sealed class DevelopmentMailSender(ILogger<DevelopmentMailSender> logger) : IMailSenderService
{
    public Task SendAsync(string recipient, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("DEMO MAIL to {Recipient}\nSubject: {Subject}\n{Body}", recipient, subject, body);
        return Task.CompletedTask;
    }
}
