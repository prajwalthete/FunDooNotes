namespace ModelLayer.Models.Collaboration
{
    public class CollaborationInfoModel
    {
        public int CollaborationId { get; set; }
        public int UserId { get; set; }
        public int NoteId { get; set; }
        public string CollaboratorEmail { get; set; }

    }
}
