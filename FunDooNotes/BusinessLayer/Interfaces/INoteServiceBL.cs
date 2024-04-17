using ModelLayer.Models.Note;

namespace BusinessLayer.Interfaces
{
    public interface INoteServiceBL
    {
        Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId);
        Task<NoteResponse> UpdateNoteAsync(int noteId, int UserId, CreateNoteRequest updatedNote);
        Task<bool> DeleteNoteAsync(int noteId, int userId);
        Task<IEnumerable<NoteResponse>> GetAllNoteAsync(int userId);
        Task<bool> IsArchivedAsync(int UserId, int NoteId);
        Task<bool> MoveToTrashAsync(int UserId, int NoteId);
        Task<NoteResponse> GetNoteByIdAsync(int NoteId, int UserId);




    }
}
