using System.Net;
using System.Net.Mail;

namespace SafeBit.Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var client = new SmtpClient(settings["Host"])
            {
                Port = int.Parse(settings["Port"]!),
                Credentials = new NetworkCredential(
                    settings["Username"],
                    settings["Password"]
                ),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(
                    settings["SenderEmail"]!,
                    settings["SenderName"]
                ),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await client.SendMailAsync(message);
        }
    }
}
