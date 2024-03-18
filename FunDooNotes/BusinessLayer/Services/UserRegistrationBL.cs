using BusinessLayer.Interfaces;
using ModelLayer.Models;
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

        public Task<bool> ForgetPassword(string email)
        {
            return _userRegistrationRl.ForgetPassword(email);
        }

        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel)
        {

            return _userRegistrationRl.RegisterUser(userRegistrationModel);
        }

        public Task<bool> ResetPassword(string newPassWord, int UserId)
        {
            return _userRegistrationRl.ResetPassword(newPassWord, UserId);
        }

        public Task<string> UserLogin(UserLoginModel userLogin)
        {
            return _userRegistrationRl.UserLogin(userLogin);
        }




    }
}