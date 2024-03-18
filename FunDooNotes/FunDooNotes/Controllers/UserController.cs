using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;
using System.Security.Claims;

namespace FunDooNotes.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRegistrationBL _registrationBL;
        private readonly IEmailBL _emailBL;

        public UserController(IUserRegistrationBL registrationBL, IEmailBL emailBL)
        {
            _registrationBL = registrationBL;
            _emailBL = emailBL;

        }


        [HttpPost]
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

                var response = new ResponseModel<string>
                {
                    StatusCode = 200,
                    Message = "Login Sucessfull",
                    Data = token

                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                if (ex is NotFoundException)
                {
                    var response = new ResponseModel<UserLoginModel>
                    {
                        StatusCode = 409,
                        IsSuccess = false,
                        Message = ex.Message

                    };
                    return Conflict(response);
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
            // Extract user Email and UserId claims from the token
            var userEmailClaim = User.FindFirstValue(ClaimTypes.Email);
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userEmailClaim == null)
            {
                return Unauthorized("Invalid token");
            }
            if (userIdClaim == null)
            {
                return Unauthorized("Invalid token");
            }

            // Compare the user email and Id from the token with the expected values
            if (!expectedUserEmail.Equals(userEmailClaim))
            {
                return Unauthorized("You are not authorized to access this resource.");
            }

            // This endpoint can only be accessed with a valid JWT token and the correct user email
            return Ok("Welcome to the FundooNotes!");
        }


        /*
        [HttpGet("sendMail")]
        public async Task<IActionResult> GetEmail(string to)
        {
            try
            {
                // Define email subject and message
                string subject = "Mail Subject";
                string message = "Hello Prajwal, Welcome To the Dot Net Core Web Api SMTP SendEmail";

                // Call the email service to send the email
                var result = await _emailBL.SendEmailAsync(to, subject, message);

                // Check if the email was sent successfully
                if (result)
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode = 200,
                        Message = "Email sent successfully.",
                       // Data = token

                    };
                    return Ok(response);
                  
                }
                else
                {
                    var response = new ResponseModel<UserLoginModel>
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Failed to send email."
                    };
                    return BadRequest(response);
                 
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");

                // Return internal server error response
                return StatusCode(500, new { Message = "Internal server error." });
            }
        }

        */

        [HttpPost("send")]
        public async Task<IActionResult> GetEmail(string to)
        {
            try
            {
                string subject = "Mail Subject";
                string message = "Hello Prajwal, Welcome To the Dot Net Core Web Api SMTP SendEmail";

                bool isEmailSent = await _emailBL.SendEmailAsync(to, subject, message);

                if (isEmailSent)
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode = 200,
                        Message = "Email sent successfully.",
                        Data = "Email sent successfully will end token."
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode = 400,
                        Message = "Failed to send email.",
                        Data = null
                    };
                    return BadRequest(response);
                }
            }
            catch (EmailSendingException ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Message = $"Error sending email: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    Message = $"An unexpected error occurred: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }








    }
}