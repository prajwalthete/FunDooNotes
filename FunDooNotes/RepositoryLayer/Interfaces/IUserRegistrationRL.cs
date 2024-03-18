using ModelLayer.Models;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRegistrationRL
    {
        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel);

        public Task<string> UserLogin(UserLoginModel userLogin);

        public Task<bool> ForgetPassword(string email);

        public Task<bool> ResetPassword(string newPassWord, int UserId);



    }
}