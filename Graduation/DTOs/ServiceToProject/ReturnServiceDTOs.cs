using Graduation.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.ServiceToProject
{
    public class ReturnServiceDTOs
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double PriceRange { get; set; }
        public int UsersID { get; set; }
        public int TypeId { get; set; }
        public int AddressId { get; set; }
    }
}
