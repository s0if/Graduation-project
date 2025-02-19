namespace Graduation.Model
{
    public class TypeService
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
        public IEnumerable<ServiceProject> Services { get; set; } = new HashSet<ServiceProject>();
    }
}
