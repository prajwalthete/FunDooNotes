using ModelLayer.Models;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRegistrationRL
    {
        public Task<bool> AddNewUser(UserRegistrationModel userRegistrationModel);

        public Task<bool> UserLogin(UserLoginModel userLogin);
        public Task<UserRegistrationModel> AuthenticateUser(string email, string password);


    }
}