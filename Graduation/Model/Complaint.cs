using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class Complaint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool status { get; set; }
        public DateTime CreatedDate { get; set; }= DateTime.Now;

        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }

        public IEnumerable<ImageDetails> ImageDetails { get; set; }=new HashSet<ImageDetails>();

    }
}
