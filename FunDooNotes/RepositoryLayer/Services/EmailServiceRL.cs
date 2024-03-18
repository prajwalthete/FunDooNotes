using ModelLayer.Models.Email;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.Interfaces;
using System.Net;
using System.Net.Mail;

namespace RepositoryLayer.Services

{
    public class EmailServiceRL : IEmailRL
    {
        private readonly EmailSettings _emailSettings;

        public EmailServiceRL()
        {
        }

        public EmailServiceRL(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }


        public async Task<bool> SendEmailAsync(string to, string subject, string htmlMessage)
        {
            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.FromEmail),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                // Console.WriteLine($"SMTP error occurred: {smtpEx.Message}");
                throw new EmailSendingException("SMTP error occurred while sending email", smtpEx);
            }
            catch (InvalidOperationException invalidOpEx)
            {
                // Console.WriteLine($"Invalid operation error occurred: {invalidOpEx.Message}");
                throw new EmailSendingException("Invalid operation occurred while sending email", invalidOpEx);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
