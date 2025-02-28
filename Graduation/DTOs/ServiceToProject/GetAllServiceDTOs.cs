using Graduation.DTOs.Images;
using Graduation.DTOs.Reviews;
using Graduation.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.ServiceToProject
{
    public class GetAllServiceDTOs
    {


        public int Id { get; set; }
        public int? userId { get; set; }
        public string? UserName { get; set; }
        public string Description { get; set; }
        public double PriceRange { get; set; }
        public string TypeName { get; set; }

        public string AddressName { get; set; }

        public List<GetImageDTOs> ImageDetails { get; set; } 
        public List<GetAllReviewDTOs> Reviews { get; set; } 


    }
}
