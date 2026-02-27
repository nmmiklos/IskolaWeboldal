using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class Gallery
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A kollekció címének megadása kötelező.")]
        public string Title { get; set; }

        public List<Image> Images { get; set; } = new List<Image>();
    }
}