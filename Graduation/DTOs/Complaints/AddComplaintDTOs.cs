namespace Graduation.DTOs.Complaints
{
    public class AddComplaintDTOs
    {
        public string NameComplaint { get; set; }
        public string Content { get; set; }
        public IFormFile? Image { get; set; }
    }
}
