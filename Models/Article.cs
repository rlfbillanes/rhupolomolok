using System;
using System.ComponentModel.DataAnnotations;

namespace rhupolomolok.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; } // CKEditor HTML

        [Required]
        public string Category { get; set; }

        public string Status { get; set; } // Draft, Pending, Approved, Rejected

        public string CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsSubmitted { get; set; } = false;

    }
}
