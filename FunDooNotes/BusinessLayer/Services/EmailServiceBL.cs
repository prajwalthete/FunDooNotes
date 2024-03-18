using BusinessLayer.Interfaces;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class EmailServiceBL : IEmailBL
    {
        private readonly IEmailRL emailRepository;

        public EmailServiceBL(IEmailRL emailRepository)
        {
            this.emailRepository = emailRepository;
        }

        public Task<bool> SendEmailAsync(string to, string subject, string htmlMessage)
        {
            return emailRepository.SendEmailAsync(to, subject, htmlMessage);
        }
    }
}
