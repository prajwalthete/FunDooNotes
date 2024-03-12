using ModelLayer.Models;

namespace RepositoryLayer.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(UserLoginModel user);

    }
}
