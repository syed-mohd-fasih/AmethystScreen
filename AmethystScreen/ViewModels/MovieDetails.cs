using AmethystScreen.Models;

namespace AmethystScreen.ViewModels
{
    public class MovieDetails
    {
        public required Movie Movie { get; set; }
        public required bool IsLikedByUser { get; set; }
        public required List<Comment> Comments { get; set; }
        public CommentsModel CommentsModel { get; set; }
    }

    public class CommentsModel
    {
        public Comment? currComment { get; set; }
        public string? AuthenticatedUserId { get; set; }
    }
}
