using ModelLayer.Models;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRegistrationRL
    {
        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel);

        public Task<string> UserLogin(UserLoginModel userLogin);
        // public Task<UserLoginModel> AuthenticateUser(string email, string password);


    }
}