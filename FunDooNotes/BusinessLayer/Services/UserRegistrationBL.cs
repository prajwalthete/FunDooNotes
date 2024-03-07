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
            // return Task.FromResult(true);
            return _userRegistrationRl.AddNewUser(userRegistrationModel);
        }
    }
}
