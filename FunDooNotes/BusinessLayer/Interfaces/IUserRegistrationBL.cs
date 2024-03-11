using ModelLayer.Models;

namespace BusinessLayer.Interfaces
{
    public interface IUserRegistrationBL
    {
        public Task<bool> AddNewUser(UserRegistrationModel userRegistrationModel);
        public Task<bool> UserLogin(UserLoginModel userLogin);

    }
}