using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using ModelLayer.Models;
using ModelLayer.Models.Note;
using RepositoryLayer.GlobleExceptionhandler;
using System.Security.Claims;
using System.Text.Json;

namespace FunDooNotes.Controllers
{
    [Route("api/note")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteServiceBL _noteServiceBL;
        private readonly IDistributedCache _distributedCache;

        public NoteController(INoteServiceBL noteServiceBL, IDistributedCache distributedCache)
        {
            _noteServiceBL = noteServiceBL;
            _distributedCache = distributedCache;
        }

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> AddNote(CreateNoteRequest createNoteRequest)
        //{
        //    try
        //    {
        //        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        int userId = Convert.ToInt32(userIdClaim);

        //        // Create the note and get the list of all notes
        //        var allNotes = await _noteServiceBL.CreateNoteAndGetNotesAsync(createNoteRequest, userId);

        //        // Update cache for GetAllNotes method
        //        var getAllNotesCacheKey = $"Notes_{userId}";
        //        var options = new DistributedCacheEntryOptions();
        //        await _distributedCache.SetAsync(getAllNotesCacheKey, JsonSerializer.SerializeToUtf8Bytes(allNotes), options);

        //        // Prepare response with the updated list of all notes
        //        var response = new ResponseModel<IEnumerable<NoteResponse>>
        //        {
        //            Message = "Note Created Successfully",
        //            Data = allNotes
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        var response = new ResponseModel<IEnumerable<NoteResponse>>
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = null // Ensure Data is null in case of error
        //        };
        //        return Ok(response);
        //    }
        //}



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddNote(CreateNoteRequest createNoteRequest)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int userId = Convert.ToInt32(userIdClaim);

                var note = await _noteServiceBL.CreateNoteAndGetNotesAsync(createNoteRequest, userId);

                var response = new ResponseModel<IEnumerable<NoteResponse>>
                {
                    Message = "Note Created Successfully",
                    Data = note
                };
                return Ok(response);


            }
            catch (Exception ex)
            {
                var response = new ResponseModel<IEnumerable<NoteResponse>>
                {
                    Success = false,
                    Message = ex.Message,

                };
                return Ok(response);

            }
        }


        [Authorize]
        [HttpPut("{noteId}")]
        public async Task<IActionResult> UpdateNoteAsync(int noteId, [FromBody] CreateNoteRequest updatedNote)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                var cacheKey = $"User_{userId}_Notes";
                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                Dictionary<int, NoteResponse> notes;

                if (cachedNotes != null)
                {
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);

                    if (notes.ContainsKey(noteId))
                    {
                        // Update the note in cache
                        notes[noteId].Title = updatedNote.Title;
                        notes[noteId].Description = updatedNote.Description;
                        notes[noteId].Colour = updatedNote.Colour;

                    }
                    else
                    {
                        notes = new Dictionary<int, NoteResponse>();
                    }
                }
                else
                {
                    notes = new Dictionary<int, NoteResponse>();
                }

                var updatedNoteResponse = await _noteServiceBL.UpdateNoteAsync(noteId, userId, updatedNote);

                // Update the note in cache
                notes[noteId] = updatedNoteResponse;

                var optionsForCache = new DistributedCacheEntryOptions();
                await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), optionsForCache);

                // Update the cache for GetAllNotes as well
                var getAllNotesCacheKey = $"Notes_{userId}";
                await _distributedCache.RemoveAsync(getAllNotesCacheKey);

                var response = new ResponseModel<NoteResponse>
                {
                    Message = "Note updated successfully",
                    Data = updatedNoteResponse
                };

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                var response = new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message
                };
                return NotFound(response);
            }
            catch (DatabaseException ex)
            {
                var response = new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message
                };
                return StatusCode(500, response);
            }
            catch (RepositoryException ex)
            {
                var response = new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message
                };
                return StatusCode(500, response);
            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred: " + ex.Message
                };
                return StatusCode(500, response);
            }
        }


        [Authorize]
        [HttpDelete("{noteId}")]
        public async Task<IActionResult> DeleteNoteAsync(int noteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                // Fetch all notes from cache
                var cacheKey = $"User_{userId}_Notes";
                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                Dictionary<int, NoteResponse> notes;

                if (cachedNotes != null)
                {
                    // Deserialize cached notes
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);

                    // Check if the note to be deleted exists in the cache
                    if (notes.ContainsKey(noteId))
                    {
                        // Remove the note from the cache
                        notes.Remove(noteId);

                        // Update the cache for GetAllNotes
                        var getAllNotesCacheKey = $"Notes_{userId}";
                        await _distributedCache.RemoveAsync(getAllNotesCacheKey);

                        // Update the cache for individual note
                        var options = new DistributedCacheEntryOptions();
                        await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);
                    }
                }

                // Delete the note
                bool isDeleted = await _noteServiceBL.DeleteNoteAsync(noteId, userId);

                if (isDeleted)
                {
                    return Ok(new ResponseModel<string>
                    {
                        Message = "Note deleted successfully",
                        Data = null
                    });
                }
                else
                {
                    return NotFound(new ResponseModel<string>
                    {
                        Success = false,
                        Message = "Note not found",
                        Data = null
                    });
                }
            }
            catch (DatabaseException ex)
            {
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    Data = null
                });
            }
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);
                var cacheKey = $"Notes_{userId}";

                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                IEnumerable<NoteResponse> notes;

                if (cachedNotes != null)
                {
                    // If notes are found in the cache, deserialize and filter out archived and Trashed notes
                    notes = JsonSerializer.Deserialize<IEnumerable<NoteResponse>>(cachedNotes);

                    // Filter out archived notes
                    notes = notes.Where(note => !note.IsDeleted && !note.IsDeleted);

                    return Ok(new ResponseModel<IEnumerable<NoteResponse>>
                    {
                        Message = "Notes retrieved successfully from caching",
                        Data = notes
                    });
                }
                else
                {
                    // If notes are not found in the cache, fetch from the service layer
                    notes = await _noteServiceBL.GetAllNoteAsync(userId);

                    if (notes != null && notes.Any())
                    {
                        // Filter out archived notes
                        notes = notes.Where(note => !note.IsDeleted);

                        // Cache the fetched notes
                        var options = new DistributedCacheEntryOptions();
                        await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);

                        return Ok(new ResponseModel<IEnumerable<NoteResponse>>
                        {
                            Message = "Notes retrieved successfully from Db",
                            Data = notes
                        });
                    }
                    else
                    {
                        return Ok(new ResponseModel<IEnumerable<NoteResponse>>
                        {
                            Message = "No notes found",
                            Data = Enumerable.Empty<NoteResponse>()
                        });
                    }
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving notes from the database.",
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred.",
                    Data = null
                });
            }
        }




        [Authorize]
        [HttpGet("GetNoteById")]
        public async Task<IActionResult> GetNoteById(int noteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                var cacheKey = $"User_{userId}";

                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                Dictionary<int, NoteResponse> notes = null; // Initialize notes

                if (cachedNotes != null)
                {
                    // If notes are found in the cache, deserialize and return them
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);
                }

                // Check if the note with the given ID is found in the cache
                if (notes != null && notes.ContainsKey(noteId))
                {
                    // If the note with the given ID is found in the cache, fetch the note from the database to compare
                    var cachedNote = notes[noteId];
                    var noteFromDb = await _noteServiceBL.GetNoteByIdAsync(noteId, userId);

                    if (noteFromDb != null && IsNoteDifferent(cachedNote, noteFromDb))
                    {
                        // If the note in the cache is different from the one in the database, remove the old note from the cache
                        notes.Remove(noteId);
                    }
                    else
                    {
                        // If the note in the cache matches the one in the database, return it from the cache
                        return Ok(new ResponseModel<NoteResponse>
                        {
                            Message = "Note retrieved successfully from caching",
                            Data = cachedNote
                        });
                    }
                }

                // If note not found in cache or cache is not present, fetch from the service layer
                var note = await _noteServiceBL.GetNoteByIdAsync(noteId, userId);

                if (note != null)
                {
                    // Update cache with the fetched note
                    notes ??= new Dictionary<int, NoteResponse>(); // Initialize notes if null
                    notes[noteId] = note;

                    var options = new DistributedCacheEntryOptions();
                    await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);

                    return Ok(new ResponseModel<NoteResponse>
                    {
                        Message = "Note retrieved successfully from Db",
                        Data = note
                    });
                }
                else
                {
                    return NotFound(new ResponseModel<NoteResponse>
                    {
                        Success = false,
                        Message = "Note not found",
                        Data = null
                    });
                }
            }
            catch (SqlException)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving notes from the database.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred." + ex.Message,
                    Data = null
                });
            }
        }

        // Method to check if two notes are different
        private bool IsNoteDifferent(NoteResponse note1, NoteResponse note2)
        {
            // Compare relevant properties to determine if the notes are different
            return note1.Title != note2.Title ||
                   note1.Description != note2.Description ||
                   note1.Colour != note2.Colour ||
                   note1.IsArchived != note2.IsArchived ||
                   note1.IsDeleted != note2.IsDeleted;
        }



        [Authorize]
        [HttpPatch("IsArchived")]
        public async Task<IActionResult> IsArchived(int NoteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                // Fetch all notes from cache
                var cacheKey = $"User_{userId}_Notes";
                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                Dictionary<int, NoteResponse> notes;

                if (cachedNotes != null)
                {
                    // Deserialize cached notes
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);

                    // Check if the note exists in cache
                    if (notes.ContainsKey(NoteId))
                    {
                        // Remove the note from GetAllNotes cache
                        var getAllNotesCacheKey = $"Notes_{userId}";
                        await _distributedCache.RemoveAsync(getAllNotesCacheKey);

                        // Update the archive status of the note
                        var note = notes[NoteId];
                        bool originalArchivedStatus = note.IsDeleted;
                        note.IsDeleted = !note.IsArchived; // Toggle the archive status

                        // Cache the updated notes
                        var options = new DistributedCacheEntryOptions();
                        await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);

                        // Return success response
                        string message = note.IsDeleted ? "Note Archived successfully" : "Note UnArchived successfully";
                        return Ok(new ResponseModel<string>
                        {
                            StatusCode = 200,
                            Message = message,
                            Data = null
                        });
                    }
                }

                // If the note is not found in cache or cache itself is not available, proceed to fetch from the service layer
                var result = await _noteServiceBL.IsArchivedAsync(userId, NoteId);

                // Invalidate the cache for GetAllNotes
                var getAllNotesKey = $"Notes_{userId}";
                await _distributedCache.RemoveAsync(getAllNotesKey);

                // Return success response
                string responseMessage = result ? "Note Archived successfully" : "Note UnArchived successfully";
                return Ok(new ResponseModel<string>
                {
                    StatusCode = 200,
                    Message = responseMessage,
                    Data = null
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while Archiving note in database.",
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while archiving the Note",
                    Data = null
                });
            }
        }



        [Authorize]
        [HttpPatch("MoveToTrash")]
        public async Task<IActionResult> MoveToTrashAsync(int noteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                // Fetch all notes from cache
                var cacheKey = $"User_{userId}_Notes";
                var cachedNotes = await _distributedCache.GetAsync(cacheKey);
                Dictionary<int, NoteResponse> notes;

                if (cachedNotes != null)
                {
                    // Deserialize cached notes
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);

                    // Check if the note exists in cache
                    if (notes.ContainsKey(noteId))
                    {
                        // Remove the note from GetAllNotes cache
                        var getAllNotesCacheKey = $"Notes_{userId}";
                        await _distributedCache.RemoveAsync(getAllNotesCacheKey);

                        // Update the deleted status of the note
                        var note = notes[noteId];
                        bool originalDeletedStatus = note.IsDeleted;
                        note.IsDeleted = !note.IsDeleted; // Toggle the deleted status

                        // Cache the updated notes
                        var options = new DistributedCacheEntryOptions();
                        await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);

                        // Return success response
                        string message = note.IsDeleted ? "Note moved to trash successfully" : "Note restored successfully";
                        return Ok(new ResponseModel<string>
                        {
                            StatusCode = 200,
                            Message = message,
                            Data = null
                        });
                    }
                }

                // If the note is not found in cache or cache itself is not available, proceed to fetch from the service layer
                var result = await _noteServiceBL.MoveToTrashAsync(userId, noteId);

                // Invalidate the cache for GetAllNotes
                var getAllNotesKey = $"Notes_{userId}";
                await _distributedCache.RemoveAsync(getAllNotesKey);

                // Return success response
                string responseMessage = result ? "Note moved to trash successfully" : "Note restored successfully";
                return Ok(new ResponseModel<string>
                {
                    StatusCode = 200,
                    Message = responseMessage,
                    Data = null
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while trashing note in the database.",
                    Data = null
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while trashing the note.",
                    Data = null
                });
            }
        }


    }

}







