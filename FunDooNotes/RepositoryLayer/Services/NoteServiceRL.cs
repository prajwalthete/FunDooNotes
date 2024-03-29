﻿using Dapper;
using Microsoft.Data.SqlClient;
using ModelLayer.Models.Note;
using RepositoryLayer.Context;
using RepositoryLayer.GlobleExceptionhandler;
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
                    var note = await connection.ExecuteAsync(insertQuery, parameters);
                }
                catch (SqlException ex)
                {

                    throw new Exception("An error occurred while adding the note.", ex);
                }

                // if the table exists then only Execute SELECT query
                if (tableExists)
                {
                    //var notes = await connection.QueryAsync<NoteResponse>(selectQuery, parameters);
                    // return notes.Reverse().ToList();

                    return await GetAllNoteAsync(UserId);

                }
                else
                {
                    // Return an empty list or handle the case when the table doesn't exist
                    return new List<NoteResponse>();
                }
            }
        }



        public async Task<NoteResponse> UpdateNoteAsync(int noteId, int userId, CreateNoteRequest updatedNote)
        {

            var selectQuery = "SELECT NoteId, Description, Title, Colour FROM Notes WHERE UserId = @UserId AND NoteId = @NoteId";

            var updateQuery = @"
                UPDATE Notes 
                SET Description = @Description, 
                    Title = @Title, 
                    Colour = @Colour 
                WHERE UserId = @UserId AND NoteId = @NoteId;
            ";

            string prevTitle, prevDescription, prevColour;

            try
            {
                using (var connection = _Context.CreateConnection())
                {
                    // Retrieve the current note details from the database
                    var currentnote = await connection.QueryFirstOrDefaultAsync<NoteResponse>(selectQuery, new { UserId = userId, NoteId = noteId });


                    if (currentnote == null)
                    {
                        throw new NotFoundException("Note not found");
                    }

                    // Store the previous values of the note
                    prevTitle = currentnote.Title;
                    prevDescription = currentnote.Description;
                    prevColour = currentnote.Colour;

                    // Execute the update query with the provided parameters
                    await connection.ExecuteAsync(updateQuery, new
                    {
                        Description = CheckInput(updatedNote.Description, prevDescription),
                        Title = CheckInput(updatedNote.Title, prevTitle),
                        Colour = CheckInput(updatedNote.Colour, prevColour),
                        UserId = userId,
                        NoteId = noteId
                    });

                    // Retrieve the updated note
                    var updatedNoteResponse = await connection.QueryFirstOrDefaultAsync<NoteResponse>(selectQuery, new { UserId = userId, NoteId = noteId });

                    // If the updated note is still null, throw a custom exception
                    if (updatedNoteResponse == null)
                    {
                        throw new DatabaseException("Failed to retrieve the updated note");
                    }

                    return updatedNoteResponse;
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL exceptions
                throw new DatabaseException("An error occurred while updating the note in the database", ex);
            }
            catch (NotFoundException ex)
            {

                throw new NotFoundException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                //  other exceptions
                throw new RepositoryException("An error occurred in the repository layer", ex);
            }
        }

        private string CheckInput(string newValue, string previousValue)
        {
            return string.IsNullOrEmpty(newValue) ? previousValue : newValue;
        }


        public async Task<bool> DeleteNoteAsync(int noteId, int userId)
        {
            var deleteQuery = "DELETE FROM Notes WHERE NoteId = @NoteId AND UserId = @UserId";

            try
            {
                using (var connection = _Context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { NoteId = noteId, UserId = userId });
                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {

                throw new DatabaseException("An error occurred while deleting the note from the database", ex);
            }
            catch (Exception ex)
            {

                throw new RepositoryException("An error occurred in the repository layer", ex);
            }
        }

        public async Task<IEnumerable<NoteResponse>> GetAllNoteAsync(int userId)
        {
            var selectQuery = "SELECT * FROM Notes WHERE UserId = @UserId AND IsDeleted = 0 AND IsArchived = 0";

            // var selectQuery = "SELECT * FROM Notes WHERE UserId = @UserId ";



            using (var connection = _Context.CreateConnection())
            {
                try
                {
                    var notes = await connection.QueryAsync<NoteResponse>(selectQuery, new { UserId = userId });
                    return notes.Reverse().ToList();
                }
                catch (SqlException ex)
                {
                    throw new Exception("An error occurred while retrieving notes from the database.", ex);
                }
            }
        }



        public async Task<bool> IsArchivedAsync(int UserId, int NoteId)
        {


            var selectQuery = "SELECT IsArchived FROM Notes WHERE UserId = @UserId AND NoteId = @NoteId";
            var queryToCheckNoteIsInTrash = "SELECT IsDeleted FROM Notes WHERE UserId = @UserId AND NoteId = @NoteId";



            var toggleQuery = @"
                                 UPDATE Notes 
                                 SET IsArchived = CASE WHEN IsArchived = 0 THEN 1 ELSE 0 END
                                 WHERE UserId = @UserId AND NoteId = @NoteId AND IsDeleted = 0
                                 ";

            using (var connection = _Context.CreateConnection())
            {
                try
                {
                    var wasTrashed = await connection.ExecuteScalarAsync<bool?>(queryToCheckNoteIsInTrash, new { UserId = UserId, NoteId = NoteId });

                    if (wasTrashed == true)
                    {
                        throw new NotFoundException($"Note with NoteId '{NoteId}' does not exist for User with UserId '{UserId} to Archive beacuse it is trashed'.");
                    }

                    var wasArchived = await connection.ExecuteScalarAsync<bool?>(selectQuery, new { UserId = UserId, NoteId = NoteId });

                    if (wasArchived == null)
                    {
                        throw new NotFoundException($"Note with NoteId '{NoteId}' does not exist for User with UserId '{UserId}'.");
                    }

                    var rowsAffected = await connection.ExecuteAsync(toggleQuery, new { UserId = UserId, NoteId = NoteId });

                    return !(bool)wasArchived;

                }
                catch (SqlException ex)
                {
                    throw new Exception("An error occurred while archiving the note in the database.", ex);
                }

            }
        }


        public async Task<bool> MoveToTrashAsync(int UserId, int NoteId)
        {
            var selectQuery = "SELECT IsDeleted FROM Notes WHERE UserId = @UserId AND NoteId = @NoteId";

            var toggleQuery = @"
                            UPDATE Notes 
                            SET IsDeleted = CASE WHEN IsDeleted = 0 THEN 1 ELSE 0 END
                            WHERE UserId = @UserId AND NoteId = @NoteId
                            ";

            using (var connection = _Context.CreateConnection())
            {
                try
                {
                    // Get the current state of IsDeleted (true if note is already in trash, false otherwise)
                    var wasMovedToTrash = await connection.ExecuteScalarAsync<bool?>(selectQuery, new { UserId = UserId, NoteId = NoteId });

                    if (wasMovedToTrash == null)
                    {
                        throw new NotFoundException($"Note with NoteId '{NoteId}' does not exist for User with UserId '{UserId}'.");
                    }


                    // Toggle the IsDeleted state to move the note to trash or restore it
                    await connection.ExecuteAsync(toggleQuery, new { UserId = UserId, NoteId = NoteId });

                    // Return updated state of IsDeleted
                    return !(bool)wasMovedToTrash;
                }
                catch (SqlException ex)
                {
                    throw new Exception("An error occurred while moving note to Trash in the database.", ex);
                }
                catch (NotFoundException ex)
                {

                    throw new NotFoundException(ex.Message, ex);
                }
            }
        }

        public async Task<NoteResponse> GetNoteByIdAsync(int NoteId, int UserId)
        {
            var selectQuery = "SELECT * FROM Notes WHERE NoteId = @NoteId AND UserId = @UserId";

            using (var connection = _Context.CreateConnection())
            {
                try
                {
                    var note = await connection.QuerySingleOrDefaultAsync<NoteResponse>(selectQuery, new { UserId = UserId, NoteId = NoteId });

                    if (note == null)
                    {
                        throw new NotFoundException($"Note with NoteId '{NoteId}' does not exist for User with UserId '{UserId}'.");
                    }

                    return note;
                }
                catch (SqlException ex)
                {
                    throw new Exception("An error occurred while retriving note to from database.", ex);
                }

            }
        }
    }
}




