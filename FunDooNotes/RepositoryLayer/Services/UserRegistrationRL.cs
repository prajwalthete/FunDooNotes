using Dapper;
using ModelLayer.Models;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using System.Data;

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
            parameters.Add("Email", userRegModel.Email, DbType.String);
            parameters.Add("Password", userRegModel.Password, DbType.String);

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
                                            Email NVARCHAR(100) NOT NULL,
                                            Password NVARCHAR(100) NOT NULL
                                            )"
                                                );
                }

                // Insert new user
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }

    }
}
