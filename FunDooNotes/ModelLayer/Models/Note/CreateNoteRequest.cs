﻿using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models.Note
{
    public class CreateNoteRequest
    {
        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string Colour { get; set; } = string.Empty;
    }

}
