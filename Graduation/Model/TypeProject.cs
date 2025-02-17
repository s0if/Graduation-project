namespace Graduation.Model
{
    public class TypeProject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
        public IEnumerable<ServiceProject> Services { get; set; } = new HashSet<ServiceProject>();
    }
}
