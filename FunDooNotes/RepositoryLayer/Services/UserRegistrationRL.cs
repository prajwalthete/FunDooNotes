using Confluent.Kafka;
using Dapper;
using ModelLayer.Models;
using ModelLayer.Models.Note;
using Newtonsoft.Json;
using RepositoryLayer.Context;
using RepositoryLayer.Entities;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;
using RepositoryLayer.Interfaces;
using System.Data;
using System.Text.RegularExpressions;

namespace RepositoryLayer.Services
{
    public class UserRegistrationRL : IUserRegistrationRL
    {
        private readonly DapperContext _Context;
        private readonly IAuthService _authService;
        private readonly IEmailRL _emailService;
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;


        public UserRegistrationRL(DapperContext context, IAuthService authService, IEmailRL emailService, IProducer<string, string> producer, IConsumer<string, string> consumer)
        {
            _Context = context;
            _authService = authService;
            _emailService = emailService;
            _producer = producer;
            _consumer = consumer;
        }


        public async Task<bool> RegisterUser(UserRegistrationModel userRegistrationDto)
        {

            if (!IsValidEmail(userRegistrationDto.Email))
            {
                throw new InvalidEmailFormatException("Invalid email format");
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegistrationDto.Password);

            // Prepare parameters for database query
            var parameters = new DynamicParameters();
            parameters.Add("firstName", userRegistrationDto.FirstName, DbType.String);
            parameters.Add("lastName", userRegistrationDto.LastName, DbType.String);
            parameters.Add("email", userRegistrationDto.Email, DbType.String);
            parameters.Add("password", hashedPassword, DbType.String);

            // SQL query to insert user into the database and retrieve the generated UserId
            var insertQuery = @"
                             INSERT INTO Users (FirstName, LastName, Email, Password)
                             VALUES (@firstName, @lastName, @email, @password);
                             SELECT CAST(SCOPE_IDENTITY() as int);";

            // Create a database connection
            using (var connection = _Context.CreateConnection())
            {
                // Check if the Users table exists, create it if necessary
                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'");

                if (!tableExists)
                {
                    await connection.ExecuteAsync(@"
                CREATE TABLE Users (
                    UserId INT PRIMARY KEY IDENTITY(1,1),
                    FirstName VARCHAR(100) NOT NULL,
                    LastName VARCHAR(100) NOT NULL,
                    Email VARCHAR(100) UNIQUE NOT NULL,
                    Password VARCHAR(100) NOT NULL
                );");
                }

                // Check if the email already exists in the database
                int emailExistsCount = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Users WHERE Email = @email", parameters);

                if (emailExistsCount > 0)
                {
                    throw new DuplicateEmailException("Email address is already in use");
                }

                // Insert the user into the database and retrieve the result
                bool insertionResult = await connection.QuerySingleAsync<bool>(insertQuery, parameters);

                if (!insertionResult)
                {
                    throw new Exception("Error occurred while inserting data into the database");
                }

                // Produce user registration event to Kafka topic
                var userEventData = new
                {
                    FirstName = userRegistrationDto.FirstName,
                    LastName = userRegistrationDto.LastName,
                    Email = userRegistrationDto.Email
                };
                await _producer.ProduceAsync("user-registration-topic", new Message<string, string> { Value = JsonConvert.SerializeObject(userEventData) });

                // Subscribe to the Kafka topic for sending registration emails
                _consumer.Subscribe("user-registration-topic");

                // Handle incoming Kafka messages to send registration emails asynchronously
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var message = _consumer.Consume();

                            // Extract user registration data from Kafka message
                            var eventData = JsonConvert.DeserializeObject<UserRegistrationModel>(message.Value);

                            // Prepare email body
                            var htmlBody = $@"
                        <!DOCTYPE html>
                        <html lang='en'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Registration Successful</title>
                        </head>
                        <body>
                            <h1>Registration Successful</h1>
                            <p>Hello, {eventData.FirstName}</p>
                            <p>Your registration was successful. You can now login to your account.</p>
                            <p>Best regards,<br>Your Application Team</p>
                        </body>
                        </html>";

                            // Send registration email
                            await _emailService.SendEmailAsync(eventData.Email, "Registration Successful", htmlBody);

                            // Log success message
                            // Console.WriteLine($"Email sent for user registration: {eventData.Email}");
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occurred while consuming Kafka message: {e.Error.Reason}");
                            throw; // Rethrow
                        }
                    }
                });

                // Return the result of the database insertion
                return insertionResult;
            }
        }



        public async Task<string> UserLogin(UserLoginModel userLogin)
        {

            var parameters = new DynamicParameters();
            parameters.Add("Email", userLogin.Email);


            string query = @"
                            SELECT UserId, Email, Password -- Add more fields if needed
                            FROM Users 
                            WHERE Email = @Email;
                            ";

            using (var connection = _Context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(query, parameters);

                if (user == null)
                {
                    throw new NotFoundException($"User with email '{userLogin.Email}' not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
                {
                    throw new InvalidPasswordException($"User with Password '{userLogin.Password}' not Found.");
                }

                //if password enterd from user and password in db match then generate Token 
                var token = _authService.GenerateJwtToken(user);
                return token;
            }
        }


        public bool IsValidEmail(string email)
        {
            // string pattern = @"^[a-zA-Z]([\w]|\.[\w]+)\@[a-zA-Z0-9]+\.[a-z]{2,3}$";
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        public async Task<string> ForgetPassword(ForgetPasswordModel forgetPasswordModel)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Email", forgetPasswordModel.Email);


            string query = @"
                            SELECT UserId, Email, Password -- Add more fields if needed
                            FROM Users 
                            WHERE Email = @Email;
                            ";

            using (var connection = _Context.CreateConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(query, parameters);

                if (user == null)
                {
                    throw new NotFoundException($"User with email '{forgetPasswordModel.Email}' not found.");
                }
                //if password enterd from user and password in db match then generate Token 
                var _token = _authService.GenerateJwtToken(user);


                // Generate password reset link
                var resetPasswordUrl = $"https://localhost:7258/api/User/ResetPassword?token={_token}";

                var emailBody = $@"
                            <html>
                                 <head>
                                         <style>
                                              .reset-link {{
                                              color: blue;
                                              font-weight: bold;
                                              }}
                                         </style>
                                 </head>
                            <body>
                                        <p>Hello,</p>
                                        <p>Please click on the following link to reset your password:</p>
                                         <p>
                                         <a href=""{resetPasswordUrl}"" class=""reset-link"">{resetPasswordUrl} </a>
                                         </p>
                                         <p>Thank you!</p>
                            </body>
                            </html>
                                   ";


                // Send email with HTML body
                await _emailService.SendEmailAsync(forgetPasswordModel.Email, "Reset Password", emailBody);

                return _token;

            }
        }


        public async Task<bool> ResetPassword(string NewPassword, int UserId)
        {
            try
            {
                var query = @"
                                 SELECT *
                                 FROM Users
                                 WHERE UserId = @UserId;
                                  ";

                using (var connection = _Context.CreateConnection())
                {

                    var user = await connection.QueryFirstOrDefaultAsync<UserRegistrationModel>(query, new { UserId = UserId });

                    if (user == null)
                    {
                        throw new NotFoundException($"User with ID '{UserId}' not found.");
                    }


                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);


                    user.Password = hashedPassword;



                    var updateQuery = @"
                                         UPDATE Users
                                         SET Password = @Password
                                         WHERE UserId = @UserId;
                                          ";

                    await connection.ExecuteAsync(updateQuery, new { Password = hashedPassword, UserId = UserId });

                    return true;
                }
            }
            catch (Exception ex)
            {

                throw new RepositoryException("An error occurred while resetting the password.", ex);
            }
        }

    }
}