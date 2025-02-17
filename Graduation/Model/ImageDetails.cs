using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class ImageDetails
    {
        public int Id { get; set; }
        public string Image { get; set; }

        [ForeignKey(nameof(Properties))]
        public int PropertyId { get; set; }
        public PropertyProject Properties { get; set; }

        [ForeignKey(nameof(Services))]
        public int ServiceId { get; set; }
        public ServiceProject Services { get; set; }
    }
}
