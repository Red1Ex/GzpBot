namespace Gzpbot.Models
{
    public class Vacancy
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<User> AppliedUsers { get; set; } = new List<User>();
    }
}
