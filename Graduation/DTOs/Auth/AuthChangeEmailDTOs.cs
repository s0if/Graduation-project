using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthChangeEmailDTOs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
