using ModelLayer.Models.Note;

namespace RepositoryLayer.Interfaces
{
    public interface INoteServiceRL
    {

        Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId);


    }
}
