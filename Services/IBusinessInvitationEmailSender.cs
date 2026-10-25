using System.Threading;
using System.Threading.Tasks;

namespace SparkVenture.Services;

/// <summary>
/// Application-specific email adapter. Replace the SMTP implementation with your
/// chosen email provider when needed. Never log or persist the raw invite link.
/// </summary>
public interface IBusinessInvitationEmailSender
{
    bool IsConfigured { get; }

    Task SendAsync(
        string recipientEmail,
        string businessName,
        string role,
        string acceptanceUrl,
        CancellationToken cancellationToken);
}
