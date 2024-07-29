using AmethystScreen.Models;

namespace AmethystScreen.ViewModels
{
    public class MovieDetails
    {
        public required Movie Movie { get; set; }
        public required List<Comment> Comments { get; set; }
    }
}
