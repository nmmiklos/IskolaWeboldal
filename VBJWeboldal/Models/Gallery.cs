using System.Collections.Generic;

namespace VBJWeboldal.Models
{
    public class Gallery
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public List<Image> Images { get; set; }
    }
}
