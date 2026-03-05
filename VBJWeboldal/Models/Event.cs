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

        [Required(ErrorMessage = "Dátum megadása kötelező.")] //6Le6goAsAAAAAIZoVZd_c671tjubp9oA-ug94Yx8
        public DateTime EventDate { get; set; }
    }
}