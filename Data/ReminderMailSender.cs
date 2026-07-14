using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ReachingOutDB.Data
{
    // Thin wrapper around MailKit so the background service (and, later, a "send test email"
    // button) don't have to repeat the connect/authenticate/send/disconnect dance.
    public class ReminderMailSender
    {
        private readonly ILogger<ReminderMailSender> logger;

        public ReminderMailSender(ILogger<ReminderMailSender> logger)
        {
            this.logger = logger;
        }

        public async Task SendAsync(SmtpSetting settings, string recipientEmails, string subject, string body, CancellationToken ct = default)
        {
            // Distinct (case-insensitive) so a repeated or copy-pasted address in the
            // RecipientEmails field can't cause the same mailbox to receive several copies -
            // some SMTP relays deliver once per RCPT TO rather than deduplicating themselves.
            var toAddresses = recipientEmails
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (toAddresses.Count == 0)
            {
                throw new InvalidOperationException("No recipient email addresses configured for this reminder.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.FromName ?? settings.FromAddress, settings.FromAddress));
            foreach (var address in toAddresses)
            {
                message.To.Add(MailboxAddress.Parse(address));
            }
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            var socketOptions = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(settings.Host, settings.Port, socketOptions, ct);

            if (!string.IsNullOrWhiteSpace(settings.Username))
            {
                await client.AuthenticateAsync(settings.Username, settings.Password, ct);
            }

            // Once SendAsync returns without throwing, the SMTP server has accepted the message -
            // it has genuinely been sent. A failure while disconnecting afterward doesn't change
            // that, so it must not be allowed to look like the send itself failed: the caller
            // treats an exception from this method as "nothing was sent, safe to retry," and
            // retrying a message that already went out is exactly how duplicates happen.
            await client.SendAsync(message, ct);

            try
            {
                await client.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "SMTP disconnect failed after the message was already sent successfully - ignoring.");
            }
        }
    }
}
