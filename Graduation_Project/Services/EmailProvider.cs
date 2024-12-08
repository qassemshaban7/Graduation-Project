using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace Graduation_Project.Services
{
    public class EmailProvider : IEmailProvider
    {
        private readonly IConfiguration _config;

        public EmailProvider(IConfiguration config)
        {
            _config = config;
        }
        public async Task<int> SendResetCode(string to)
        {
            var email = new MimeMessage();
            Random rnd = new Random();
            int pin = rnd.Next(1, 9999);
            email.From.Add(MailboxAddress.Parse(_config["stmp:Email"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = "Reset password";
            string pinStr = pin.ToString();
            while (pinStr.Length < 4)
            {
                pinStr = "0" + pinStr;
            }
            email.Body = new TextPart(TextFormat.Plain)
            {
                Text = $"your verification code is {pinStr}"
            };
            var smtp = new SmtpClient();
            smtp.Connect(_config["stmp:Host"],
                int.Parse(_config["stmp:Port"]),
                SecureSocketOptions.StartTls);
            smtp.Authenticate(_config["stmp:Email"], _config["stmp:Pass"]);
            smtp.Send(email);
            smtp.Disconnect(true);
            return pin;
        }
    }
}
