using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class ApplicationUser :IdentityUser<int>
    {
        public string? ConfirmationCode { get; set; }
        public DateTime? ConfirmationCodeExpiry { get; set; } 
        public string? CurrentTokenId { get; set; }
        [ForeignKey(nameof(Address))]
        public int? AddressId { get; set; }
        public AddressToProject? Address { get; set; }
        public IEnumerable<Complaint> Complaints { get; set; }=new HashSet<Complaint>();
        public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
        public IEnumerable<ServiceProject> Services { get; set; }= new HashSet<ServiceProject>();
        public IEnumerable<Review> Reviews { get; set; }=new HashSet<Review>();
        public IEnumerable<SaveProject> Saves { get; set; }=new HashSet<SaveProject>();
        public IEnumerable<ChatMessage> ChatMessages { get; set; }=new HashSet<ChatMessage>();

    }
}
