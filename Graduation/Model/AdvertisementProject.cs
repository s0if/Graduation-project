namespace Graduation.Model
{
    public class AdvertisementProject
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }  
        public DateTime EndAt { get; set; }
        public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
        public IEnumerable<ServiceProject> Services { get; set; } = new HashSet<ServiceProject>();
    }
}
