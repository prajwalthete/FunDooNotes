using Dapper;
using ModelLayer.Models;
using RepositoryLayer.Context;
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

        public UserRegistrationRL(DapperContext context)
        {
            _Context = context;
        }

        public async Task<bool> AddNewUser(UserRegistrationModel userRegModel)
        {
            //if (string.IsNullOrWhiteSpace(userRegModel?.FirstName) ||
            //    string.IsNullOrWhiteSpace(userRegModel?.LastName) ||
            //    string.IsNullOrWhiteSpace(userRegModel?.Email) ||
            //    string.IsNullOrWhiteSpace(userRegModel?.Password))
            //{
            //    throw new ArgumentException("Input fields cannot be empty");
            //    // or return BadRequest("Input fields cannot be empty");
            //}


            var queryCheckEmail = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Email = @Email;
            ";

            var parametersCheckEmail = new DynamicParameters();
            parametersCheckEmail.Add("Email", userRegModel.Email, DbType.String);

            var query = @"
                          INSERT INTO Users (FirstName, LastName, Email, Password)
                          VALUES (@FirstName, @LastName, @Email, @Password);
                        ";

            var parameters = new DynamicParameters();
            parameters.Add("FirstName", userRegModel.FirstName, DbType.String);
            parameters.Add("LastName", userRegModel.LastName, DbType.String);


            if (!IsValidEmail(userRegModel.Email))
            {
                Console.WriteLine("Invalid email format");

                throw new InvalidEmailFormatException("Invalid email format");
            }

            parameters.Add("Email", userRegModel.Email, DbType.String);

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
                bool emailExists = await connection.QueryFirstOrDefaultAsync<bool>(queryCheckEmail, parametersCheckEmail);

                if (emailExists)
                {

                    throw new DuplicateEmailException("Email address is already in use");

                }

                // Insert new user
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }

        public async Task<bool> UserLogin(UserLoginModel userLogin)
        {
            using (var connection = _Context.CreateConnection())
            {
                string query = @"
                        SELECT * FROM Users WHERE Email = @Email ;
                       ";

                var parameters = new DynamicParameters();
                parameters.Add("Email", userLogin.Email);

                var user = await connection.QueryFirstOrDefaultAsync<UserRegistrationModel>(query, parameters);

                if (user == null)
                {
                    throw new UserNotFoundException($"User with email '{userLogin.Email}' not found.");
                }

                if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password))
                {
                    throw new InvalidPasswordException($"User with Password '{userLogin.Password}' not Found.");
                }

                return true;
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
