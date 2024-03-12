using RepositoryLayer.Entities;

namespace RepositoryLayer.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(UserEntity user);

    }
}
