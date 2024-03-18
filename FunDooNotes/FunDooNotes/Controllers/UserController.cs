using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.Models;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FunDooNotes.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRegistrationBL _registrationBL;
        private readonly IConfiguration _config;

        public UserController(IUserRegistrationBL registrationBL, IConfiguration config)//, IEmailBL emailBL)
        {
            _registrationBL = registrationBL;
            _config = config;

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




        //[HttpPost("send")]
        //public async Task<IActionResult> GetEmail(string to)
        //{
        //    try
        //    {
        //        string subject = "Mail Subject";
        //        string message = "Hello Prajwal, Welcome To the Dot Net Core Web Api SMTP SendEmail";

        //        bool isEmailSent = await _emailBL.SendEmailAsync(to, subject, message);

        //        if (isEmailSent)
        //        {
        //            var response = new ResponseModel<string>
        //            {
        //                StatusCode = 200,
        //                Message = "Email sent successfully.",
        //                Data = "Email sent successfully will end token."
        //            };
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            var response = new ResponseModel<string>
        //            {
        //                StatusCode = 400,
        //                IsSuccess = false,
        //                Message = "Failed to send email.",
        //                Data = null
        //            };
        //            return BadRequest(response);
        //        }
        //    }
        //    catch (EmailSendingException ex)
        //    {
        //        var response = new ResponseModel<string>
        //        {
        //            StatusCode = 500,
        //            IsSuccess = false,
        //            Message = $"Error sending email: {ex.Message}",
        //            Data = null
        //        };
        //        return StatusCode(500, response);
        //    }
        //    catch (Exception ex)
        //    {
        //        var response = new ResponseModel<string>
        //        {
        //            IsSuccess = false,
        //            StatusCode = 500,
        //            Message = $"An unexpected error occurred: {ex.Message}",
        //            Data = null
        //        };
        //        return StatusCode(500, response);
        //    }
        //}




        [HttpPost("sendemail")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            try
            {

                bool isEmailSent = await _registrationBL.ForgetPassword(email);

                if (isEmailSent)
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode = 200,
                        Message = "Email sent successfully.",

                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ResponseModel<string>
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Failed to send email.",
                        Data = null
                    };
                    return BadRequest(response);
                }
            }
            catch (NotFoundException ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Error sending email: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
            catch (EmailSendingException ex)
            {
                var response = new ResponseModel<string>
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Error sending email: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"An unexpected error occurred: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
        }



        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string token, string password)
        {
            try
            {

                var handler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JwtSettings:Issuer"],
                    ValidAudience = _config["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]))
                };

                SecurityToken validatedToken;
                var principal = handler.ValidateToken(token, validationParameters, out validatedToken);

                // Extract claims
                var userId = principal.FindFirstValue("UserId");
                int _userId = Convert.ToInt32(userId);



                bool isPassWordReset = await _registrationBL.ResetPassword(password, _userId);

                //  bool isPassWordReset = await _registrationBL.ResetPassword(password, 1006);


                var response = new ResponseModel<bool>
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Password reset successfully",
                    Data = isPassWordReset
                };

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                var response = new ResponseModel<bool>
                {
                    StatusCode = 404,
                    IsSuccess = false,
                    Message = ex.Message,
                    Data = false
                };

                return NotFound(response);
            }
            catch (RepositoryException ex)
            {
                var response = new ResponseModel<bool>
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = ex.Message,
                    Data = false
                };

                return StatusCode(500, response);
            }
            catch (Exception ex)
            {
                var response = new ResponseModel<bool>
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "An unexpected error occurred.",
                    Data = false
                };

                return StatusCode(500, response);
            }
        }





    }
}