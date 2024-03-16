using Dapper;
using ModelLayer.Models.Collaboration;
using RepositoryLayer.Context;
using RepositoryLayer.GlobleExceptionhandler;
using RepositoryLayer.GlobleExeptionhandler;
using RepositoryLayer.Interfaces;
using System.Data;
using System.Text.RegularExpressions;

namespace RepositoryLayer.Services
{
    public class CollaborationRL : ICollaborationRL
    {

        private readonly DapperContext _context;

        public CollaborationRL(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CollaborationInfoModel>> GetAllCollaborators()
        {
            var query = "SELECT * FROM Collaboration";
            using (var connection = _context.CreateConnection())
            {
                var collaborators = await connection.QueryAsync<CollaborationInfoModel>(query);
                return collaborators;

            }

        }

        public async Task<bool> RemoveCollaborator(int NoteId, CollaborationRequestModel Request, int UserId)
        {
            var query = @"
                 DELETE FROM Collaboration
                 WHERE UserId = @userId
                 AND NoteId = @NoteId
                 AND CollaboratorEmail = @collaboratorEmail;
                 ";

            var emailExistsQuery = @"
                 SELECT COUNT(*)
                 FROM Users
                 WHERE Email = @collaboratorEmail;
                 ";

            var parameters = new DynamicParameters();
            parameters.Add("UserId", UserId, DbType.Int32);
            parameters.Add("NoteId", NoteId, DbType.Int32);
            parameters.Add("collaboratorEmail", Request.Email, DbType.String);

            var emailExistsParams = new { collaboratorEmail = Request.Email };

            using (var connection = _context.CreateConnection())
            {
                int emailCount = await connection.ExecuteScalarAsync<int>(emailExistsQuery, emailExistsParams);

                if (emailCount == 0)
                {
                    throw new NotFoundException($"Collaborator with email '{Request.Email}' not found.");

                }

                int rowsAffected = await connection.ExecuteAsync(query, parameters);
                return rowsAffected > 0;

            }
        }

        public async Task<bool> AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId)
        {
            var query = @"
                            INSERT INTO Collaboration (UserId, NoteId, CollaboratorEmail) 
                            VALUES (@userId, @NoteId, @collaboratorEmail);
                            ";

            var parameters = new DynamicParameters();
            parameters.Add("userId", UserId, DbType.Int32);
            parameters.Add("NoteId", NoteId, DbType.Int32);

            if (!IsValidEmail(Request.Email))
            {
                throw new InvalidEmailFormatException("Invalid email format");
            }
            parameters.Add("collaboratorEmail", Request.Email, DbType.String);

            var emailExistsQuery = @"
                                     SELECT COUNT(*)
                                     FROM Users
                                     WHERE Email = @collaboratorEmail;
                                     ";

            var emailExistsParams = new { collaboratorEmail = Request.Email };

            using (var connection = _context.CreateConnection())
            {
                int emailCount = await connection.ExecuteScalarAsync<int>(emailExistsQuery, emailExistsParams);

                if (emailCount == 0)
                {
                    throw new NotFoundException($"Collaborator with email '{Request.Email}' Is Not A Registerd User please Register First and try Again.");
                }
                // Check if table exists
                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(
                    @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'Collaboration';
                    "
                );

                // Create table if it doesn't exist
                if (!tableExists)
                {
                    await connection.ExecuteAsync(
                        @"CREATE TABLE Collaboration (
                            CollaborationId INT IDENTITY(1, 1) PRIMARY KEY,
                            UserId INT,
                            NoteId INT,
                            CollaboratorEmail NVARCHAR(100),
                            CONSTRAINT FK_UserId FOREIGN KEY (UserId) REFERENCES Users (UserId),
                            CONSTRAINT FK_NoteId FOREIGN KEY (NoteId) REFERENCES Notes (NoteId),
                            CONSTRAINT FK_CollaboratorEmail FOREIGN KEY (CollaboratorEmail) REFERENCES Users (Email)
                        );"
                    );
                }

                // Insert collaborator
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
