using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthChangePasswordDTOs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string OldPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string NewPassword { get; set; }

    }
}
