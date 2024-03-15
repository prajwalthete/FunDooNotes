using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepositoryLayer.Entities
{
    public class Collaboration
    {
        [Key]
        public int CollaborationId { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }

        [ForeignKey("Notes")]
        public int NoteId { get; set; }

        public string CollaboratorEmail { get; set; }

    }
}
