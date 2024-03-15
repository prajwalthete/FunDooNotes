using BusinessLayer.Interfaces;
using ModelLayer.Models.Collaboration;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class CollaborationBL : ICollaborationBL
    {
        private readonly ICollaborationRL collaborationRL;

        public CollaborationBL(ICollaborationRL collaborationRL)
        {
            this.collaborationRL = collaborationRL;
        }

        public Task<bool> AddCollaborator(int NoteId, CollaborationRequestModel Request, int UserId)
        {
            return collaborationRL.AddCollaborator(NoteId, Request, UserId);

        }

        public Task<IEnumerable<CollaborationInfoModel>> GetAllCollaborators()
        {
            return collaborationRL.GetAllCollaborators();
        }
    }
}
