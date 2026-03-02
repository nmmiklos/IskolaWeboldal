using System;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A dokumentum nevének megadása kötelező.")]
        public string Title { get; set; }

        public string FilePath { get; set; }
        public string FileExtension { get; set; } // pl. ".pdf", ".docx"
        public long FileSize { get; set; } // Méret bájtban

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Ha true, akkor publikus (kifelé linkelhető), ha false, akkor csak belső (tanári)
        public bool IsPublic { get; set; }
    }
}