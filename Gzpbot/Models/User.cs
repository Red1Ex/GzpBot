namespace Gzpbot.Models
{
    public class User
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string PhoneNumber { get; set; }
        public string Profession { get; set; }
        public List<Competency> Competencies { get; set; } = new List<Competency>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Vacancy> AppliedVacancies { get; set; } = new List<Vacancy>();
    }
}