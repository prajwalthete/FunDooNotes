using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using ModelLayer.Models.Note;
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
                    StatusCode = 200,
                    Message = "Note Created Successfully",
                    Data = note
                };
                return Ok(response);


            }
            catch (Exception ex)
            {
                var response = new ResponseModel<IEnumerable<NoteResponse>>
                {
                    StatusCode = 500,
                    Message = ex.Message,

                };
                return Ok(response);

            }
        }
    }
}
