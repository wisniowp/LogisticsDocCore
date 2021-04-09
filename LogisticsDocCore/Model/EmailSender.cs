using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
//using SendGrid;
//using SendGrid.Helpers.Mail;
using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace LogisticsDocCore.Model
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
    }
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            //return Execute(Options.SendGridKey, subject, message, email);
            return Execute(subject, message, email);
        }

       // public Task Execute(string apiKey, string subject, string message, string email)
        public Task Execute(string subject, string bodymessage, string email)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("Admin",
            "awizacje@awizacjestena.pl");
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress(email,
            email);
            message.To.Add(to);

            message.Subject = subject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = bodymessage;
            //bodyBuilder.TextBody = bodymessage;
            message.Body = bodyBuilder.ToMessageBody();
            SmtpClient client = new SmtpClient();
            client.Connect("serwer2071911.home.pl", 465, true);
            client.Authenticate("awizacje@awizacjestena.pl", "Q7LWbhnt");
            client.Send(message);
            client.Disconnect(true);
            client.Dispose();
            return Task.FromResult(0);
            /*
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("Joe@contoso.com", Options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
            */
        }
    }
}
