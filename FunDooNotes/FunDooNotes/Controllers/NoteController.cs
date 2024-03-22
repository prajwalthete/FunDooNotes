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

                var updatedNoteResponse = await _noteServiceBL.UpdateNoteAsync(noteId, userId, updatedNote);


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


                bool isDeleted = await _noteServiceBL.DeleteNoteAsync(noteId, userId);

                if (isDeleted)
                {
                    return Ok(new ResponseModel<string>
                    {

                        Message = "Note deleted  successfully",
                        Data = null,

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
                    // If notes are found in the cache, deserialize and return them
                    notes = JsonSerializer.Deserialize<IEnumerable<NoteResponse>>(cachedNotes);
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
                Dictionary<int, NoteResponse> notes;

                if (cachedNotes != null)
                {
                    // If notes are found in the cache, deserialize and return them
                    notes = JsonSerializer.Deserialize<Dictionary<int, NoteResponse>>(cachedNotes);

                    if (notes.ContainsKey(noteId))
                    {
                        // If the note with the given ID is found, return it
                        var note = notes[noteId];
                        return Ok(new ResponseModel<NoteResponse>
                        {
                            Message = "Note retrieved successfully from caching",
                            Data = note
                        });
                    }
                    else
                    {
                        // If the note with the given ID is not found, return not found
                        return NotFound(new ResponseModel<NoteResponse>
                        {
                            Success = false,
                            Message = "Note not found",
                            Data = null
                        });
                    }
                }
                else
                {
                    // If notes are not found in the cache, fetch from the service layer
                    var allNotes = await _noteServiceBL.GetAllNoteAsync(userId);

                    if (allNotes != null && allNotes.Any())
                    {
                        // Convert the notes into a dictionary with note ID as key
                        notes = allNotes.ToDictionary(note => note.NoteId);

                        // Cache the fetched notes
                        var options = new DistributedCacheEntryOptions();
                        await _distributedCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(notes), options);

                        if (notes.ContainsKey(noteId))
                        {
                            // If the note with the given ID is found, return it
                            var note = notes[noteId];
                            return Ok(new ResponseModel<NoteResponse>
                            {
                                Message = "Note retrieved successfully from Db",
                                Data = note
                            });
                        }
                        else
                        {
                            // If the note with the given ID is not found, return not found
                            return NotFound(new ResponseModel<NoteResponse>
                            {
                                Success = false,
                                Message = "Note not found",
                                Data = null
                            });
                        }
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
        [HttpPatch("IsArchived")]
        public async Task<IActionResult> IsArchived(int NoteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                var result = await _noteServiceBL.IsArchivedAsync(userId, NoteId);

                // Check if the note was moved to trash or restored
                string message = result ? "Note Archived successfully" : "Note UnArchived successfully";

                return Ok(new ResponseModel<string>
                {
                    StatusCode = 200,
                    Message = message,
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
        public async Task<IActionResult> MoveToTrashAsync(int NoteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                var result = await _noteServiceBL.MoveToTrashAsync(userId, NoteId);

                string message = result ? "Note Trashed successfully" : "Note Untrashed successfully";

                return Ok(new ResponseModel<string>
                {

                    Message = message,
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
                    Message = "An error occurred while Trashing note in database.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while Trashing the Note",
                    Data = null
                });
            }
        }

    }

}







