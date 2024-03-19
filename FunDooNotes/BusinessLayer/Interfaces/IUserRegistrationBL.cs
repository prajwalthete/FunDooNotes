using ModelLayer.Models;
using ModelLayer.Models.Note;

namespace BusinessLayer.Interfaces
{
    public interface IUserRegistrationBL
    {
        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel);

        public Task<string> UserLogin(UserLoginModel userLogin);
        public Task<string> ForgetPassword(ForgetPasswordModel forgetPasswordModel);

        public Task<bool> ResetPassword(string NewPassword, int UserId);


    }
}
