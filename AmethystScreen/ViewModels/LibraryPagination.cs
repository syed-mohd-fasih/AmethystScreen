using AmethystScreen.Models;

namespace AmethystScreen.ViewModels
{
    public class LibraryPagination
    {
        public List<Movie>? Movies { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
