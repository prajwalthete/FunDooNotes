using ModelLayer.Models;

namespace BusinessLayer.Interfaces
{
    public interface IUserRegistrationBL
    {
        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel);

        public Task<string> UserLogin(UserLoginModel userLogin);
        public Task<bool> ForgetPassword(string email);

        public Task<bool> ResetPassword(string newPassWord, int UserId);


    }
}
