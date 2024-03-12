using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;
using System.Security.Claims;

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
        public async Task<IActionResult> UserRegistration(UserRegistrationModel user)
        {
            try
            {
                var addedUser = await _registrationBL.RegisterUser(user);
                if (addedUser)
                {
                    var response = new ResponseModel<UserRegistrationModel>
                    {
                        StatusCode = 200,
                        Message = "User Registration Successful"
                    };
                    return Ok(response);
                }
                else
                {

                    return BadRequest("invalid input");
                }
            }
            catch (Exception ex)
            {
                if (ex is DuplicateEmailException)
                {
                    var response = new ResponseModel<UserRegistrationModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);
                    // return BadRequest("User with given email already exists");

                }
                else if (ex is InvalidEmailFormatException)
                {
                    var response = new ResponseModel<UserRegistrationModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = ex.Message
                    };
                    return BadRequest(response);
                    // return BadRequest("Invalid email format");
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
                // Authenticate the user and generate JWT token
                var token = await _registrationBL.UserLogin(userLogin);

                var response = new ResponseModel<UserLoginModel>
                {
                    StatusCode = 200,
                    Message = "Login Sucessfull",
                    Token = token

                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                if (ex is UserNotFoundException)
                {
                    var response = new ResponseModel<UserLoginModel>
                    {
                        StatusCode = 409,
                        IsSuccess = false,
                        Message = ex.Message

                    };
                    return Conflict(response);
                    // return Conflict($"User with email '{userLogin.Email}' not found.");
                }
                else if (ex is InvalidPasswordException)
                {
                    var response = new ResponseModel<UserLoginModel>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = ex.Message

                    };
                    return BadRequest(response);
                    // return BadRequest($"User with Password '{userLogin.Password}' not Found.");
                }
                else
                {
                    return StatusCode(500, $"An error occurred while processing the login request: {ex.Message}");

                }
            }

        }



        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedEndpoint(string expectedUserEmail)
        {
            // Extract user Email claim from the token
            var userEmailClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userEmailClaim == null)
            {
                return Unauthorized("Invalid token");
            }

            // Compare the user email from the token with the expectedEmail
            if (!expectedUserEmail.Equals(userEmailClaim))
            {
                return Unauthorized("You are not authorized to access this resource.");
            }

            // This endpoint can only be accessed with a valid JWT token and the correct user ID
            return Ok("Welcome to the FundooNotes!");

        }


    }
}