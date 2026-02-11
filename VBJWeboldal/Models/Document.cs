using System;

namespace VBJWeboldal.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
