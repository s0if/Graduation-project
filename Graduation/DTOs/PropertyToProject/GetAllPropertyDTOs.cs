using Graduation.DTOs.Images;
using Graduation.DTOs.Reviews;
using Graduation.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.PropertyToProject
{
    public class GetAllPropertyDTOs
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? UserID { get; set; }
        public string TypeName { get; set; }
        public string? userName { get; set; }
        public string AddressName { get; set; }

        public List<GetImageDTOs> ImageDetails { get; set; }
        public List<GetAllReviewDTOs> Reviews { get; set; }
    }
}
