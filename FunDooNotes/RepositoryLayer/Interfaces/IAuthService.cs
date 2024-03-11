using ModelLayer.Models;

namespace RepositoryLayer.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(UserRegistrationModel user);

    }
}
