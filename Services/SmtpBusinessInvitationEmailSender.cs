using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SparkVenture.Services;

/// <summary>
/// Basic SMTP adapter using framework libraries; convenient for a hackathon MVP.
/// SMTP passwords belong in user-secrets/environment variables, NOT source control.
/// For a production system prefer a managed transactional mail provider with retries.
/// </summary>
public sealed class SmtpBusinessInvitationEmailSender : IBusinessInvitationEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpBusinessInvitationEmailSender(IConfiguration configuration)
        => _configuration = configuration;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["InvitationMail:Host"]) &&
        !string.IsNullOrWhiteSpace(_configuration["InvitationMail:FromAddress"]) &&
        int.TryParse(_configuration["InvitationMail:Port"], out var port) &&
        port > 0 && port <= 65535;

    public async Task SendAsync(
        string recipientEmail,
        string businessName,
        string role,
        string acceptanceUrl,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Invitation email delivery is not configured.");

        var host = _configuration["InvitationMail:Host"]!;
        var port = int.Parse(_configuration["InvitationMail:Port"]!);
        var from = _configuration["InvitationMail:FromAddress"]!;
        var username = _configuration["InvitationMail:Username"];
        var password = _configuration["InvitationMail:Password"];
        var enableSsl = bool.TryParse(_configuration["InvitationMail:EnableSsl"], out var ssl) && ssl;

        using var message = new MailMessage
        {
            From = new MailAddress(from, "SparkVenture"),
            Subject = "Invitation to join a SparkVenture business",
            Body = $"You have been invited to join {businessName} on SparkVenture as {role}.\n\n" +
                   $"Open this private, single-use invitation link:\n{acceptanceUrl}\n\n" +
                   "This invitation expires in 7 days. Do not share the link.",
            IsBodyHtml = false
        };
        message.To.Add(recipientEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
