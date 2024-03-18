namespace BusinessLayer.Interfaces
{
    public interface IEmailBL
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlMessage);

    }
}
