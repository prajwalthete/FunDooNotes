using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;

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

                var response = new ResponseModel<UserRegistrationModel>
                {
                    StatusCode = 200,
                    Message = "User Registration Successful"
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                if (ex is DuplicateEmailException)
                {
                    return Conflict("User with given email already exists");
                }
                else if (ex is InvalidEmailFormatException)
                {
                    return BadRequest("Invalid email format");
                }
                else
                {
                    return StatusCode(500, $"An error occurred while adding the user: {ex.Message}");
                }
            }
        }





        [HttpPost("login")]
        public async Task<IActionResult> UserLogin(UserLoginModel userLogin)
        {
            try
            {

                var userExists = await _registrationBL.UserLogin(userLogin);

                var response = new ResponseModel<UserRegistrationModel>
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Login Sucessfull",

                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                if (ex is UserNotFoundException)
                {
                    return Conflict($"User with email '{userLogin.Email}' not found.");
                }
                else if (ex is InvalidPasswordException)
                {
                    return BadRequest($"User with Password '{userLogin.Password}' not Found.");
                }
                else
                {
                    return StatusCode(500, $"An error occurred while processing the login request: {ex.Message}");

                }
            }

        }


    }
}