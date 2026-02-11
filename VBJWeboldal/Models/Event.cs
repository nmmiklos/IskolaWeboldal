using System;
using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; }
    }
}
