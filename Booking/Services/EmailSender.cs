using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;

namespace Booking.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
   
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Booking", "mohammad.j.abuaisheh@gmail.com"));
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var client = new SmtpClient())
                {
                    // For demonstration purposes, this accepts all SSL certificates.
                    // In production, properly handle server certificate validation.
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // Connect to Gmail's SMTP server using StartTLS on port 587
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                    // Authenticate using your Gmail address and an App Password (replace the empty string)
                    await client.AuthenticateAsync("mohammad.j.abuaisheh@gmail.com", "bjwgdkkxrpkfsgxq");

                    // Send the email
                    await client.SendAsync(message);

                    // Disconnect from the SMTP server
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while sending email.", ex);
            }
        }
    }
}
