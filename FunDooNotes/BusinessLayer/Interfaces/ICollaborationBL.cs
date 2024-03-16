using ModelLayer.Models.Collaboration;

namespace BusinessLayer.Interfaces
{
    public interface ICollaborationBL
    {
        Task<bool> AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId);
        Task<IEnumerable<CollaborationInfoModel>> GetAllCollaborators();
        Task<bool> RemoveCollaborator(int NoteId, CollaborationRequestModel Request, int UserId);



    }
}
