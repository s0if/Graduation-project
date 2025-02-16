using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Graduation.Model
{
    public class ApplicationUser :IdentityUser<int>
    {
        public string? Address { get; set; }
        public IEnumerable<Complaint> Complaints { get; set; }=new HashSet<Complaint>();
        public IEnumerable<Property> Properties { get; set; } = new HashSet<Property>();
        public IEnumerable<Service> Services { get; set; }= new HashSet<Service>();
        public IEnumerable<Review> Reviews { get; set; }=new HashSet<Review>();                                                                                                                     
    }
}
