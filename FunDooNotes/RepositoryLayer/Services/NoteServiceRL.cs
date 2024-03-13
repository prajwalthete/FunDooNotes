using Dapper;
using Microsoft.Data.SqlClient;
using ModelLayer.Models.Note;
using RepositoryLayer.Context;
using RepositoryLayer.Interfaces;
using System.Data;
using static Dapper.SqlMapper;

namespace RepositoryLayer.Services
{
    public class NoteServiceRL : INoteServiceRL
    {
        private readonly DapperContext _Context;

        public NoteServiceRL(DapperContext context)
        {
            _Context = context;
        }


        public async Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId)
        {
            var parameters = new DynamicParameters();

            parameters.Add("Description", createNoteRequest.Description, DbType.String);
            parameters.Add("Title", createNoteRequest.Title, DbType.String);
            parameters.Add("Colour", createNoteRequest.Colour, DbType.String);
            parameters.Add("IsArchived", false, DbType.Boolean);
            parameters.Add("IsDeleted", false, DbType.Boolean);
            parameters.Add("UserId", UserId, DbType.Int32);


            var insertQuery = @"
                                INSERT INTO Notes (Description, [Title], Colour, IsArchived, IsDeleted, UserId)
                                VALUES (@Description, @Title, @Colour, @IsArchived, @IsDeleted, @UserId);
                               ";


            var selectQuery = "SELECT * FROM Notes WHERE UserId = @UserId";


            using (var connection = _Context.CreateConnection())
            {

                bool tableExists = await connection.QueryFirstOrDefaultAsync<bool>(

                                 @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notes';");


                // Check if the table exists          
                if (!tableExists)
                {
                    await connection.ExecuteAsync(@"
                                                    CREATE TABLE Notes (
                                                             NoteId int PRIMARY KEY IDENTITY,
                                                             Description nvarchar(max),
                                                             [Title] nvarchar(max) NOT NULL,
                                                             Colour nvarchar(max),
                                                             IsArchived bit DEFAULT (0),
                                                             IsDeleted bit DEFAULT (0),
                                                             UserId int FOREIGN KEY REFERENCES Users(UserId)
                                                            );
                                                ");
                }

                try
                {
                    await connection.ExecuteAsync(insertQuery, parameters);
                }
                catch (SqlException ex)
                {

                    throw new Exception("An error occurred while adding the note.", ex);
                }

                // if the table exists then only Execute SELECT query
                if (tableExists)
                {
                    var notes = await connection.QueryAsync<NoteResponse>(selectQuery, parameters);
                    return notes.Reverse().ToList();
                }
                else
                {
                    // Return an empty list or handle the case when the table doesn't exist
                    return new List<NoteResponse>();
                }
            }
        }

    }
}
