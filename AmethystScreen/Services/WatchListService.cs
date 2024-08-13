using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;

namespace AmethystScreen.Services
{
    public class WatchListService(AppDbContext context, ILogger<LikesService> logger)
    {
        public AppDbContext _context { get; set; } = context;
        public ILogger<LikesService> _logger { get; set; } = logger;
        public async Task AddToList(string movieSlug, string userId)
        {
            ToWatch toWatch = new();
            if (!_context.Likes.Any(watch => watch.UserId == userId && watch.MovieSlug == movieSlug))
            {
                var movie = _context.Movies.FirstOrDefault(m => m.Slug == movieSlug);
                if (movie != null)
                {
                    toWatch.MovieSlug = movieSlug;
                    toWatch.UserId = userId;

                    _context.ToWatch.Add(toWatch);
                    await _context.SaveChangesAsync();
                    return;
                }
                _logger.LogWarning($"{nameof(AddToList)}: {movieSlug} not found? Like request cancelled");
                return;
            }
            _logger.LogWarning($"{nameof(AddToList)}: Like already exists? Like request cancelled");
        }

        public async Task RemoveFromList(string movieSlug, string userId)
        {
            if (_context.ToWatch.Any(watch => watch.UserId == userId && watch.MovieSlug == movieSlug))
            {
                var movie = _context.Movies.FirstOrDefault(m => m.Slug == movieSlug);
                if (movie != null)
                {
                    var toWatch = await _context.ToWatch.Where(watch => watch.MovieSlug == movieSlug && watch.UserId == userId).FirstOrDefaultAsync();
                    if (toWatch != null)
                    {
                        _context.ToWatch.Remove(toWatch);
                        await _context.SaveChangesAsync();
                        return;
                    }
                    _logger.LogWarning($"{nameof(RemoveFromList)}: Like not found" +
                        $"? Remove Like request cancelled");
                }
                _logger.LogWarning($"{nameof(RemoveFromList)}: {movieSlug} not found? Remove Like request cancelled");
                return;
            }
            _logger.LogWarning($"{nameof(RemoveFromList)}: Like doesnt exist? Remove Like request cancelled");
        }

        public async Task<bool> IsAddedToList(string movieSlug, string userId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(movieSlug))
                return false;

            return await _context.ToWatch.AnyAsync(watch => watch.UserId == userId && watch.MovieSlug == movieSlug);
        }
    }
}
