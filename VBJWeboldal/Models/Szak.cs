using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class Szak
    {
        public int Id { get; set; }

        [Required]
        public string Slug { get; set; }   // pl: gepeszet

        [Required]
        public string Name { get; set; }   // pl: Gépészet

        public string HeroClass { get; set; }

        public string HeroImageClass { get; set; }

        public string PageContent { get; set; }  // IDE kerül az egész HTML tartalom
    }
}
