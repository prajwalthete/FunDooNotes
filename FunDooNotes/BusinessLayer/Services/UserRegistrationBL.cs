using BusinessLayer.Interfaces;
using ModelLayer.Models;
using ModelLayer.Models.Note;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class UserRegistrationBL : IUserRegistrationBL
    {
        private readonly IUserRegistrationRL _userRegistrationRl;

        public UserRegistrationBL(IUserRegistrationRL userRegistrationRl)
        {
            _userRegistrationRl = userRegistrationRl;
        }

        public Task<string> ForgetPassword(ForgetPasswordModel forgetPasswordModel)
        {
            return _userRegistrationRl.ForgetPassword(forgetPasswordModel);
        }

        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel)
        {

            return _userRegistrationRl.RegisterUser(userRegistrationModel);
        }

        public Task<bool> ResetPassword(string NewPassword, int UserId)
        {
            return _userRegistrationRl.ResetPassword(NewPassword, UserId);
        }

        public Task<string> UserLogin(UserLoginModel userLogin)
        {
            return _userRegistrationRl.UserLogin(userLogin);
        }




    }
}