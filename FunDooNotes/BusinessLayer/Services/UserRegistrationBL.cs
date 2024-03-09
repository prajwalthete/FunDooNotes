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

        public Task<bool> AddNewUser(UserRegistrationModel userRegistrationModel)
        {

            return _userRegistrationRl.AddNewUser(userRegistrationModel);
        }

        public Task<bool> UserLogin(UserLoginModel userLogin)
        {
            return _userRegistrationRl.UserLogin(userLogin);
        }
    }
}
