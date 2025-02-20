using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class SaveProject
    {
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }


        
        [ForeignKey(nameof(Properties))]
        public int? PropertyId { get; set; }
        public PropertyProject? Properties { get; set; }

        [ForeignKey(nameof(Services))]
        public int? ServiceId { get; set; }
        public ServiceProject? Services { get; set; }
    }
}
