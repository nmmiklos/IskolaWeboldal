using System;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Az esemény címének megadása kötelező.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Az esemény leírásának megadása kötelező.")]
        public string Description { get; set; } // Ez jelenik meg Hover-re

        [Required(ErrorMessage = "Dátum megadása kötelező.")]
        public DateTime EventDate { get; set; }
    }
}