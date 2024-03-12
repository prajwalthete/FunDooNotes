using Dapper;
using ModelLayer.Models;
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

        public UserRegistrationRL(DapperContext context, IAuthService authService)
        {
            _Context = context;
            _authService = authService;
        }

        public async Task<bool> RegisterUser(UserRegistrationModel userRegModel)
        {

            var parametersToCheckEmailIsValid = new DynamicParameters();
            parametersToCheckEmailIsValid.Add("Email", userRegModel.Email, DbType.String);

            var querytoCheckEmailIsNotDuplicate = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Email = @Email;
            ";


            var query = @"
                          INSERT INTO Users (FirstName, LastName, Email, Password)
                          VALUES (@FirstName, @LastName, @Email, @Password);
                        ";

            var parameters = new DynamicParameters();
            parameters.Add("FirstName", userRegModel.FirstName, DbType.String);
            parameters.Add("LastName", userRegModel.LastName, DbType.String);

            //Check Emailformat Using Regex
            if (!IsValidEmail(userRegModel.Email))
            {
                Console.WriteLine("Invalid email format");

                throw new InvalidEmailFormatException("Invalid email format");
            }

            parameters.Add("Email", userRegModel.Email, DbType.String);

            //convert Plain Password into crytpographic String 
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegModel.Password);

            parameters.Add("Password", hashedPassword, DbType.String);



            using (var connection = _Context.CreateConnection())
            {

                // Check if table exists
                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(
                    @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'Users';
                     "
                );

                // Create table if it doesn't exist
                if (!tableExists)
                {
                    await connection.ExecuteAsync(
                                                    @" CREATE TABLE Users (
                                                             Id INT IDENTITY(1, 1) PRIMARY KEY,
                                                             FirstName NVARCHAR(100) NOT NULL,
                                                             LastName NVARCHAR(100) NOT NULL,
                                                             Email NVARCHAR(100) UNIQUE NOT NULL,
                                                             Password NVARCHAR(100) UNIQUE NOT NULL )"
                                                 );
                }

                // Check if email already exists
                bool emailExists = await connection.QueryFirstOrDefaultAsync<bool>(querytoCheckEmailIsNotDuplicate, parametersToCheckEmailIsValid);

                if (emailExists)
                {

                    throw new DuplicateEmailException("Email address is already in use");

                }

                // Insert new user
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }



        public async Task<string> UserLogin(UserLoginModel userLogin)
        {
            using (var connection = _Context.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("Email", userLogin.Email);


                //string query = @"
                //        SELECT * FROM Users WHERE Email = @Email ;
                //       ";
                string query = @"
                            SELECT Id, Email, Password -- Add more fields if needed
                            FROM Users 
                            WHERE Email = @Email;
                            ";

                var user = await connection.QueryFirstOrDefaultAsync<UserEntity>(query, parameters);

                if (user == null)
                {
                    throw new UserNotFoundException($"User with email '{userLogin.Email}' not found.");
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


        private bool IsValidEmail(string email)
        {

            // string pattern = @"^[a-zA-Z]([\w]|\.[\w]+)\@[a-zA-Z0-9]+\.[a-z]{2,3}$";
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";



            return Regex.IsMatch(email, pattern);
        }
    }
}