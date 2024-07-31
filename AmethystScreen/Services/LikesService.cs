using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;

namespace AmethystScreen.Services
{
    public class LikesService(AppDbContext context, ILogger<LikesService> logger)
    {
        public AppDbContext _context { get; set; } = context;
        public ILogger<LikesService> _logger { get; set; } = logger;
        public async Task AddLike(string movieSlug, string userId)
        {
            Like like = new();
            if (!_context.Likes.Any(like => like.UserId == userId && like.MovieSlug == movieSlug))
            {
                var movie = _context.Movies.FirstOrDefault(m => m.Slug == movieSlug);
                if(movie != null)
                {
                    like.MovieSlug = movieSlug;
                    like.UserId = userId;

                    _context.Likes.Add(like);
                    movie.Likes++;
                    await _context.SaveChangesAsync();
                    return;
                }
                _logger.LogWarning($"{nameof(AddLike)}: {movieSlug} not found? Like request cancelled");
                return;
            }
            _logger.LogWarning($"{nameof(AddLike)}: Like already exists? Like request cancelled");
        }

        public async Task RemoveLike(string movieSlug, string userId)
        {
            if (_context.Likes.Any(like => like.UserId == userId && like.MovieSlug == movieSlug))
            {
                var movie = _context.Movies.FirstOrDefault(m => m.Slug == movieSlug);
                if (movie != null)
                {
                    var like = await _context.Likes.Where(like => like.MovieSlug == movieSlug && like.UserId == userId).FirstOrDefaultAsync();
                    if(like != null)
                    {
                        _context.Likes.Remove(like);
                        movie.Likes--;
                        await _context.SaveChangesAsync();
                        return;
                    }
                    _logger.LogWarning($"{nameof(RemoveLike)}: Like not found" +
                        $"? Remove Like request cancelled");
                }
                _logger.LogWarning($"{nameof(AddLike)}: {movieSlug} not found? Remove Like request cancelled");
                return;
            }
            _logger.LogWarning($"{nameof(RemoveLike)}: Like doesnt exist? Remove Like request cancelled");
        }

        public async Task<bool> IsLiked(string movieSlug, string userId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(movieSlug))
                return false;

            return await _context.Likes.AnyAsync(like => like.UserId == userId && like.MovieSlug == movieSlug);
        }
    }
}
