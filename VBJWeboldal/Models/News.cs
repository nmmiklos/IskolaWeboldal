using System;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.Now;
    }
}
