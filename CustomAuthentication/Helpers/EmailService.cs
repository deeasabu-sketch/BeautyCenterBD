using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace CustomAuthentication.Helpers
{
    /// <summary>
    /// Central SMTP mail helper. Configure the Smtp:* appSettings before production use.
    /// Gmail users should use an App Password, not their normal account password.
    /// </summary>
    public static class EmailService
    {
        private static string Setting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static SmtpClient CreateClient()
        {
            var host = Setting("SmtpHost");
            var portText = Setting("SmtpPort");
            var username = Setting("SmtpUsername");
            var password = Setting("SmtpPassword");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("SMTP is not configured. Set SmtpHost, SmtpPort, SmtpUsername and SmtpPassword in Web.config.");
            }

            int port = 587;
            int.TryParse(portText, out port);
            if (port <= 0) port = 587;

            return new SmtpClient(host, port)
            {
                EnableSsl = !string.Equals(Setting("SmtpEnableSsl"), "false", StringComparison.OrdinalIgnoreCase),
                Credentials = new NetworkCredential(username, password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000
            };
        }

        private static string FromEmail => Setting("SmtpFromEmail") ?? Setting("SmtpUsername");
        private static string FromName => Setting("SmtpFromName") ?? "BeautyCenterBD";

        public static void Send(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("Recipient email is required.", nameof(to));

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(FromEmail, FromName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Body = htmlBody;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;

                using (var client = CreateClient())
                {
                    client.Send(message);
                }
            }
        }
    }
}