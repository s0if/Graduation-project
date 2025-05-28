using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class AdvertisementProject
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }  
        public DateTime EndAt { get; set; }


        [ForeignKey(nameof(service))]
        public int? serviceId {  get; set; }
        public ServiceProject? service {  get; set; }
        [ForeignKey(nameof(property))]
        public int? propertyId { get; set; }
        public PropertyProject? property { get; set; }
    }
}
