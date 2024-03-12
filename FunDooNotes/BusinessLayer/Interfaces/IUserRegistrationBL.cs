using ModelLayer.Models;

namespace BusinessLayer.Interfaces
{
    public interface IUserRegistrationBL
    {
        public Task<bool> RegisterUser(UserRegistrationModel userRegistrationModel);

        public Task<string> UserLogin(UserLoginModel userLogin);


    }
}
