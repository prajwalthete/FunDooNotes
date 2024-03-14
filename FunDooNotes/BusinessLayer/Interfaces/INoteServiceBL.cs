﻿using ModelLayer.Models.Note;

namespace BusinessLayer.Interfaces
{
    public interface INoteServiceBL
    {
        Task<IEnumerable<NoteResponse>> CreateNoteAndGetNotesAsync(CreateNoteRequest createNoteRequest, int UserId);

        Task<NoteResponse> UpdateNoteAsync(int noteId, int UserId, CreateNoteRequest updatedNote);
        Task<bool> DeleteNoteAsync(int noteId, int userId);


    }
}
