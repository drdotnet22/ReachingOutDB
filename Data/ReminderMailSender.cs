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

        public async Task SendAsync(SmtpSetting settings, string recipientEmails, string subject, string body)
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

            // switched away from async for smtpclient because it was sending 4 duplicate messages.
            using (var client = new SmtpClient())
            {
                var socketOptions = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                client.Connect(settings.Host, settings.Port, socketOptions);
                client.Authenticate(settings.Username, settings.Password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
