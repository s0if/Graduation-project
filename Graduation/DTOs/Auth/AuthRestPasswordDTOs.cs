using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthRestPasswordDTOs
    {

        [EmailAddress]
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string NewPassword { get; set; }
        [Required]
        public string code { get; set; }
    }
}
