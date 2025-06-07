using Graduation.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.DTOs.PropertyToProject
{
    public class AddPropertyDTOs
    {
        public string Description { get; set; }
        public double Price { get; set; }
        public int TypeId { get; set; }
        public int AddressId { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }
    }
}
