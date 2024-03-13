namespace ModelLayer.Models.Note
{
    public class CreateNoteRequest
    {

        public string Title { get; set; }

        public string? Description { get; set; }

        public string Colour { get; set; } = string.Empty;
    }

}
