namespace Graduation.DTOs.PropertyToProject
{
    public class ReturnPropertyDTOs
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime updateAt { get; set; }
        public double Price { get; set; }
        public int TypeId { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }
        public int userId { get; set; }
        public int addressId { get; set; }
    }
}
