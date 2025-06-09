using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class PropertyProject
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime updateAt { get; set; }
        public double Price { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }

        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }




        [ForeignKey(nameof(Type))]
        public int TypeId { get; set; }
        public TypeProperty Type { get; set; }
        [ForeignKey(nameof(Address))]
        public int AddressId { get; set; }
        public AddressToProject Address { get; set; }
        public IEnumerable<ImageDetails> ImageDetails { get; set; }=new HashSet<ImageDetails>();
        public IEnumerable<Review> Reviews { get; set; } =new HashSet<Review>();
         public IEnumerable<SaveProject> Saves { get; set; }=new HashSet<SaveProject>();
        
    }
}
