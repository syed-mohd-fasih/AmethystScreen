using AmethystScreen.Models;
using Microsoft.Identity.Client;

namespace AmethystScreen.ViewModels
{
    public class MovieDetails
    {
        public required Movie Movie { get; set; }
        public required List<Comment> Comments { get; set; }
        public required bool IsLikedByUser { get; set; }
    }
}
