using Dapper;
using ModelLayer.Models.Collaboration;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using System.Data;

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

        async Task<bool> ICollaborationRL.AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId)
        {



            var query = @"
                            INSERT INTO Collaboration (UserId, NoteId, CollaboratorsEmail) 
                            VALUES (@userId, @NoteId, @collaboratorsEmail);
                            ";

            var parameters = new DynamicParameters();
            parameters.Add("userId", UserId, DbType.Int32);
            parameters.Add("NoteId", NoteId, DbType.Int32);
            parameters.Add("collaboratorsEmail", Request.Email, DbType.String);


            using (var connection = _context.CreateConnection())
            {
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
                            CollaboratorsEmail VARCHAR(MAX),
                            CONSTRAINT FK_UserId FOREIGN KEY (UserId) REFERENCES Users (UserId),
                            CONSTRAINT FK_NoteId FOREIGN KEY (NoteId) REFERENCES Notes (NoteId)
                        );"
                    );
                }

                // Insert collaborator
                await connection.ExecuteAsync(query, parameters);
            }

            return true;
        }
    }
}
