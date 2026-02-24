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
            var senderName = settings["SenderName"] ?? "SafeBite";
            var brandedBody = BuildBrandedBody(
                subject,
                body,
                senderName
            );

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
                    senderName
                ),
                Subject = subject,
                Body = brandedBody,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await client.SendMailAsync(message);
        }

        private static string BuildBrandedBody(
            string subject,
            string contentHtml,
            string senderName
        )
        {
            var safeTitle = WebUtility.HtmlEncode(senderName);
            var safeSubject = WebUtility.HtmlEncode(subject);

            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>{safeSubject}</title>
</head>
<body style=""margin:0;padding:0;background:#ecf3f7;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;"">
    {safeSubject}
  </div>
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:26px 12px;background:linear-gradient(180deg,#f5fafc 0%,#eaf2f7 100%);"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:640px;background:#ffffff;border-radius:20px;overflow:hidden;border:1px solid #d8e5ec;box-shadow:0 14px 36px rgba(7,35,52,0.12);"">
          <tr>
            <td style=""padding:0;background:#0f766e;"">
              <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                <tr>
                  <td style=""height:6px;background:linear-gradient(90deg,#19b3a7,#72d9ca,#19b3a7);""></td>
                </tr>
                <tr>
                  <td align=""center"" style=""background:linear-gradient(135deg,#0f766e,#0ea5a4);padding:30px 20px;"">
                    <div style=""font-size:46px;font-weight:900;letter-spacing:0.7px;line-height:1;color:#ffffff;text-shadow:0 2px 12px rgba(9,68,65,0.42);"">
                      {safeTitle}
                    </div>
                    <div style=""margin-top:10px;font-size:13px;letter-spacing:2px;text-transform:uppercase;color:#d7fffa;"">
                      Smart Food Safety Alerts
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:30px 30px 20px 30px;background:#ffffff;"">
              <div style=""display:inline-block;padding:6px 12px;border-radius:999px;background:#e9faf7;color:#0d766f;font-size:12px;font-weight:700;letter-spacing:0.4px;text-transform:uppercase;"">
                Important Update
              </div>
              <h2 style=""margin:14px 0 14px 0;font-size:31px;line-height:1.2;color:#0b1d2a;font-weight:800;"">{safeSubject}</h2>
              <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background:#f7fbfd;border:1px solid #e2edf3;border-radius:14px;"">
                <tr>
                  <td style=""padding:20px 18px;line-height:1.75;font-size:16px;color:#223341;"">
                    {contentHtml}
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style=""padding:18px 30px 24px 30px;background:#f6fafc;border-top:1px solid #e5eef4;color:#4b5f6e;font-size:13px;"">
              <div style=""font-weight:700;color:#233646;"">{safeTitle} Team</div>
              <div style=""margin-top:7px;"">This is an automated message. For support, contact the SafeBite help desk.</div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
