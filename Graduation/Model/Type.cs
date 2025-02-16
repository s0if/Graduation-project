using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class Type
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<Property> Properties { get; set; } = new HashSet<Property>();
        public IEnumerable<Service> Services { get; set; } = new HashSet<Service>();
    }
}
