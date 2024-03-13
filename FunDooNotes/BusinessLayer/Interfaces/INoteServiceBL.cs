using ModelLayer.Models.Note;

namespace BusinessLayer.Interfaces
{
    public interface INoteServiceBL
    {
        Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId);

    }
}
