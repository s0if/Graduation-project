using Graduation.DTOs.PropertyToProject;
using Graduation.DTOs.ServiceToProject;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.Saves
{
    public class GetSavesDTOs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<GetAllPropertyDTOs> allProperty { get; set; }
        public List<GetAllServiceDTOs> allService { get; set; }
    }
}
