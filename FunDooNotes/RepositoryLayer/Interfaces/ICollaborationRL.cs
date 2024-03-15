using ModelLayer.Models.Collaboration;

namespace RepositoryLayer.Interfaces
{
    public interface ICollaborationRL
    {
        Task<bool> AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId);
        Task<IEnumerable<CollaborationInfoModel>> GetAllCollaborators();


    }
}
