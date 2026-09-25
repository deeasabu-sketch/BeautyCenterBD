using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomAuthentication.Models
{
    public class NotebookEntry
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string NoteText { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
