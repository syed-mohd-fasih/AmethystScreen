namespace AmethystScreen.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string PostedByUserid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }

    }
}
