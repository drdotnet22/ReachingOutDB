using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ReachingOutDB.Data
{
    // Thin wrapper around MailKit so the background service (and, later, a "send test email"
    // button) don't have to repeat the connect/authenticate/send/disconnect dance.
    public class ReminderMailSender
    {
        public async Task SendAsync(SmtpSetting settings, string recipientEmails, string subject, string body, CancellationToken ct = default)
        {
            var toAddresses = recipientEmails
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
