namespace Graduation.Model
{
    public class Advertisement
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public IEnumerable<Property> Properties { get; set; }=new HashSet<Property>();
        public IEnumerable<Service> Services { get; set; } = new HashSet<Service>();
    }
}
