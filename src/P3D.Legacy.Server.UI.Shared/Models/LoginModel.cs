using System.ComponentModel.DataAnnotations;

namespace P3D.Legacy.Server.UI.Shared.Models
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; } = default!;
    }
}