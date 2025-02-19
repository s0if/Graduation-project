using Graduation.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.PropertyToProject
{
    public class AddPropertyDTOs
    {


        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int TypeId { get; set; }
    }
}
