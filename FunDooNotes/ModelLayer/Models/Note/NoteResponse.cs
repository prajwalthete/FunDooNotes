namespace ModelLayer.Models.Note
{
    public class NoteResponse
    {

        public int NoteId { get; set; }
        public string? Description { get; set; }
        public string Title { get; set; }
        public string Colour { get; set; }
        public bool IsArchived { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

    }
}
