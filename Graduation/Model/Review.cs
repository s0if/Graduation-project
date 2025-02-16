using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class Review
    {
       public int Id { get; set; }
        public string Description { get; set; }


        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(Properties))]
        public int? PropertyId { get; set; }
        public Property? Properties { get; set; }

        [ForeignKey(nameof(Services))]
        public int? ServiceId { get; set; }
        public Service? Services { get; set; }
    }
}
