using Microsoft.AspNetCore.Identity;

namespace VBJWeboldal.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Az IdentityUser már tartalmazza az Id, Email, PasswordHash, stb. mezőket.
        // Itt csak a saját, plusz mezőket adjuk hozzá:
        public string FullName { get; set; }
        public string? Title { get; set; }
        public bool IsManagement { get; set; }
        public string? ProfileImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}