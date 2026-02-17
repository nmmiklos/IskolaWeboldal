using System;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A cím megadása kötelező.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "A tartalom megadása kötelező.")]
        public string Content { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.Now;

        // ÚJ MEZŐK:
        public bool IsPublished { get; set; } = true; // true = Publikált, false = Piszkozat

        public string? AuthorId { get; set; } // Kapcsolat a felhasználóhoz
        public ApplicationUser? Author { get; set; }
        //képek
        public string? CoverImagePath { get; set; }

    }
}