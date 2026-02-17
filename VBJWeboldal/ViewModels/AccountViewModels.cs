using System.ComponentModel.DataAnnotations;

namespace VBJWeboldal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Az email cím megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Érvénytelen email formátum.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "A teljes név megadása kötelező.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Az email cím megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Érvénytelen email formátum.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik meg.")]
        public string ConfirmPassword { get; set; }
    }
}