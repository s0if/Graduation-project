using Graduation.DTOs.Images;
using System.Dynamic;

namespace Graduation.DTOs.Complaints
{
    public class GetAllComplaintDTOs
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UsersID { get; set; }
        public List<GetImageDTOs> Images { get; set; }
    }
}
