using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.ServiceToProject;
using Graduation.Model;

namespace Graduation.DTOs.Advertisement
{
    public class GetAllAdvertisementDTOs
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public GetAllPropertyDTOs? Properties { get; set; }
        public GetAllServiceDTOs? Services { get; set; }
    }
}
