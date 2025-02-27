namespace Graduation.DTOs.Reviews
{
    public class GetAllReviewDTOs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string description { get; set; }
        public double rating { get; set; }
        public DateTime date { get; set; }

    }
}
