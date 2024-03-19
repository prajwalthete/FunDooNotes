using BusinessLayer.Interfaces;
using ModelLayer.Models.Note;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class NoteServiceBL : INoteServiceBL
    {
        private readonly INoteServiceRL _noteServiceRL;

        public NoteServiceBL(INoteServiceRL noteServiceRL)
        {
            _noteServiceRL = noteServiceRL;
        }

        public Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId)
        {
            return _noteServiceRL.CreateNoteAndGetNotesAsync(createNoteRequest, UserId);
        }



        Task<NoteResponse> INoteServiceBL.UpdateNoteAsync(int noteId, int UserId, CreateNoteRequest updatedNote)
        {
            return _noteServiceRL.UpdateNoteAsync(noteId, UserId, updatedNote);

        }

        public Task<bool> DeleteNoteAsync(int noteId, int userId)
        {
            return _noteServiceRL.DeleteNoteAsync(noteId, userId);

        }

        public Task<IEnumerable<NoteResponse>> GetAllNoteAsync(int userId)
        {
            return _noteServiceRL.GetAllNoteAsync(userId);
        }

        public Task<bool> IsArchivedAsync(int UserId, int NoteId)
        {
            return _noteServiceRL.IsArchivedAsync(UserId, NoteId);
        }

        public Task<bool> MoveToTrashAsync(int UserId, int NoteId)
        {
            return _noteServiceRL.MoveToTrashAsync(UserId, NoteId);
        }

        public Task<NoteResponse> GetNoteByIdAsync(int NoteId, int UserId)
        {
            return _noteServiceRL.GetNoteByIdAsync(NoteId, UserId);
        }
    }
}
