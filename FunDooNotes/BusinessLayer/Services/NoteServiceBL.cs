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
    }
}
