namespace Graduation.DTOs.Auth
{
    public class AuthGetAllUserDTOs
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone {  get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
    }
}
