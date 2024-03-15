using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Models;
using ModelLayer.Models.Collaboration;
using System.Security.Claims;

namespace FunDooNotes.Controllers
{
    [Route("api/collaboration")]
    [ApiController]
    public class CollaborationController : ControllerBase
    {
        public readonly ICollaborationBL collaborationBL;

        public CollaborationController(ICollaborationBL collaborationBL)
        {
            this.collaborationBL = collaborationBL;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddCollaborator(int NoteId, [FromBody] CollaborationRequestModel request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = Convert.ToInt32(userIdClaim);

                var note = await collaborationBL.AddCollaborator(NoteId, request, userId);

                var response = new ResponseModel<string>
                {
                    StatusCode = 200,
                    Message = "Collaboration Successfull",

                };
                return Ok(response);


            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,

                };
                return Ok(response);

            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCollaborator()
        {
            try
            {
                var collaborators = await collaborationBL.GetAllCollaborators();

                var response = new ResponseModel<IEnumerable<CollaborationInfoModel>>
                {
                    StatusCode = 200,
                    Message = "Collaborators Fetched Successfully",
                    Data = collaborators

                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                var response = new ResponseModel<string>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message,

                };
                return Ok(response);


            }

        }
    }
}
