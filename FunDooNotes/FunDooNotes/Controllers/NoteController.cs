using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ModelLayer.Models;
using ModelLayer.Models.Note;
using RepositoryLayer.GlobleExceptionhandler;
using System.Security.Claims;

namespace FunDooNotes.Controllers
{
    [Route("api/note")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteServiceBL noteServiceBL;

        public NoteController(INoteServiceBL noteServiceBL)
        {
            this.noteServiceBL = noteServiceBL;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddNote(CreateNoteRequest createNoteRequest)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int userId = Convert.ToInt32(userIdClaim);

                var note = await noteServiceBL.CreateNoteAndGetNotesAsync(createNoteRequest, userId);

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

                var updatedNoteResponse = await noteServiceBL.UpdateNoteAsync(noteId, userId, updatedNote);


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


                bool isDeleted = await noteServiceBL.DeleteNoteAsync(noteId, userId);

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
                var notes = await noteServiceBL.GetAllNoteAsync(userId);

                return Ok(new ResponseModel<IEnumerable<NoteResponse>>
                {
                    Message = notes != null && notes.Any() ? "Notes retrieved successfully" : "No notes found",
                    Data = notes
                });
            }
            catch (Exception ex)
            {
                if (ex is SqlException)
                {
                    return StatusCode(500, new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred while retrieving notes from the database.",
                        Data = null
                    });
                }
                else
                {
                    return StatusCode(500, new ResponseModel<string>
                    {
                        Success = false,
                        Message = "An error occurred.",
                        Data = null
                    });
                }
            }
        }

        [Authorize]
        [HttpGet("GetNoteById")]
        public async Task<IActionResult> GetNoteById(int NoteId)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int UserId = Convert.ToInt32(userIdClaim);
                var notes = await noteServiceBL.GetNoteByIdAsync(NoteId, UserId);

                if (notes != null)
                {
                    return Ok(new ResponseModel<NoteResponse>
                    {
                        Message = "Note retrieved successfully",
                        Data = notes
                    });
                }
                else
                {
                    return NotFound(new ResponseModel<NoteResponse>
                    {
                        Success = false,
                        Message = "No note found",
                        Data = null
                    });
                }
            }
            catch (NotFoundException ex)
            {
                var response = new ResponseModel<string>
                {
                    Success = false,
                    Message = $"Error sending email: {ex.Message}",
                    Data = null
                };
                return StatusCode(500, response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ResponseModel<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving note from the database.",
                    Data = null
                });
            }
            catch (Exception ex)
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

                var result = await noteServiceBL.IsArchivedAsync(userId, NoteId);

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

                var result = await noteServiceBL.MoveToTrashAsync(userId, NoteId);

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







