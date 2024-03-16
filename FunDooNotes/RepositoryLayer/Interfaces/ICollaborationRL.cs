using ModelLayer.Models.Collaboration;

namespace RepositoryLayer.Interfaces
{
    public interface ICollaborationRL
    {
        public Task<bool> AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId);

        public Task<IEnumerable<CollaborationInfoModel>> GetAllCollaborators();
        public Task<bool> RemoveCollaborator(int NoteId, CollaborationRequestModel Request, int UserId);


    }
}
