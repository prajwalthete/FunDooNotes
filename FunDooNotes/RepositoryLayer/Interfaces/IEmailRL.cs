namespace RepositoryLayer.Interfaces
{
    public interface IEmailRL
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlMessage);
    }
}
