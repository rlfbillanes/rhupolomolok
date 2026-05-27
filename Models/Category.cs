using System.ComponentModel.DataAnnotations;

namespace rhupolomolok.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? ColorHex { get; set; } = "#000000"; // default black

        [MaxLength(255)]
        public string? BannerImagePath { get; set; } // optional hero banner

        [MaxLength(50)]
        public string? LayoutStyle { get; set; } = "default";

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Article>? Articles { get; set; }
    }
}
