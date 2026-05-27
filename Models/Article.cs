using System;
using System.ComponentModel.DataAnnotations;

namespace rhupolomolok.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsSubmitted { get; set; } = false;
    }
}
