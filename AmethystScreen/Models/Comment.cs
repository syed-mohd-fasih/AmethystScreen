namespace AmethystScreen.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentByUsername { get; set; } = string.Empty;
        public string CommentById { get; set; } = string.Empty;
        public string MovieSlug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public List<Comment> Replies { get; set; } = new List<Comment>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
