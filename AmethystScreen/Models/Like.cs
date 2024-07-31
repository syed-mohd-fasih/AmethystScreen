namespace AmethystScreen.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string MovieSlug { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
