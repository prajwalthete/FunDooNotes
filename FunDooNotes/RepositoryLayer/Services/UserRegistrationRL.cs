using Dapper;
using ModelLayer.Models;
using RepositoryLayer.Context;
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
                return false;
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

                // Insert new user
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }

        public async Task<bool> UserLogin(UserLoginModel userLogin)
        {
            //if (!IsValidEmail(userLogin.Email))
            //{
            //    //Console.WriteLine("Invalid email format");
            //    return false;
            //}

            using (var connection = _Context.CreateConnection())
            {
                string query = @"
                                 SELECT * FROM Users WHERE Email = @Email ;
                                ";

                var parameters = new DynamicParameters();
                parameters.Add("Email", userLogin.Email);


                var user = await connection.QueryFirstOrDefaultAsync<UserRegistrationModel>(query, parameters);

                return user != null && BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password); ;
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
