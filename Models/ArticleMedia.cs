using System;

namespace rhupolomolok.Models
{
    public class ArticleMedia
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }
        public Article? Article { get; set; }   // Non-nullable properties addressed

        public string MediaType { get; set; } = string.Empty; // "Image" or "Video"
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
