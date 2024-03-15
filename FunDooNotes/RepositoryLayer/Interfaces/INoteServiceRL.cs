﻿using ModelLayer.Models.Note;

namespace RepositoryLayer.Interfaces
{
    public interface INoteServiceRL
    {

        Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId);
        Task<NoteResponse> UpdateNoteAsync(int noteId, int UserId, CreateNoteRequest updatedNote);
        Task<bool> DeleteNoteAsync(int noteId, int userId);
        Task<IEnumerable<NoteResponse>> GetAllNoteAsync(int userId);

        Task<bool> IsArchive(int UserId, int NoteId);


    }
}
