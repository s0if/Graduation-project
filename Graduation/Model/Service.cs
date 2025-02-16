using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class Service
    {
        public int Id { get; set; }
        public int Description { get; set; }
        public double PriceRange { get; set; }

        [ForeignKey(nameof(User))]
        public int UsersID { get; set; }
        public ApplicationUser User { get; set; }


        [ForeignKey(nameof(Advertisements))]
        public int? AdvertisementID { get; set; }
        public Advertisement? Advertisements { get; set; }


        [ForeignKey(nameof(Type))]
        public int TypeId { get; set; }
        public Type Type { get; set; }


        public IEnumerable<ImageDetails> ImageDetails { get; set; } = new HashSet<ImageDetails>();
        public IEnumerable<Review> Reviews { get; set; } = new HashSet<Review>();
    }
}
