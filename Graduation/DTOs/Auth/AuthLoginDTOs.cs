using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthLoginDTOs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }
        
    }
}
