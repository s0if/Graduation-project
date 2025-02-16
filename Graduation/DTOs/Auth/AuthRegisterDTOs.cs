using System.ComponentModel.DataAnnotations;

namespace Graduation.DTOs.Auth
{
    public class AuthRegisterDTOs
    {
        [Required]
        [MaxLength(20,ErrorMessage = "please enter name less 20 characters")]
        [MinLength(3, ErrorMessage = "please enter name more 3 characters")]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }
        [Required]
        
        public string Phone { get; set; }
        public string? Address {  get; set; }
        public string role { get; set; } = "consumer";
    }
}
