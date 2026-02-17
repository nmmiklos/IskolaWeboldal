using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "A teljes név megadása kötelező.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Az email cím megadása kötelező.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Válassz jogosultságot!")]
        public string Role { get; set; }
    }
}