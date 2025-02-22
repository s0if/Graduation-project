namespace Graduation.Model
{
    public class AddressToProject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable< ApplicationUser> Users { get; set; } =new HashSet<ApplicationUser>();
        public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
        public IEnumerable<ServiceProject> Services { get; set; } = new HashSet<ServiceProject>();

    }
}
