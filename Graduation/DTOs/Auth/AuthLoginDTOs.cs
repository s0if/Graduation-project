using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthLoginDTOs
    {

        [EmailAddress]
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }

    }
}
