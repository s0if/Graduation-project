using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class Review
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(Properties))]
        public int? PropertyId { get; set; }
        public PropertyProject? Properties { get; set; }

        [ForeignKey(nameof(Services))]
        public int? ServiceId { get; set; }
        public ServiceProject? Services { get; set; }
    }
}
