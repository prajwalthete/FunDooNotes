using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;

namespace FunDooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunDooController : ControllerBase
    {
        private readonly IUserRegistrationBL _registrationBL;

        public FunDooController(IUserRegistrationBL registrationBL)
        {
            _registrationBL = registrationBL;
        }



        [HttpPost("register")]
        public async Task<IActionResult> AddNewUser(UserRegistrationModel user)
        {
            try
            {
                var addedUser = await _registrationBL.AddNewUser(user);
                return Ok(addedUser); // Return the added user
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> UserLogin(UserLoginModel userLogin)
        {
            try
            {

                var userExists = await _registrationBL.UserLogin(userLogin);

                if (userExists)
                {

                    return Ok("Login successful");
                }
                else
                {

                    return Unauthorized("Invalid email or password");
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An error occurred while processing the login request: {ex.Message}");
            }
        }


    }
}