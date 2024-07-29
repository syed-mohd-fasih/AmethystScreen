using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace AmethystScreen.Services
{
    public class CommentsService
    {
        private readonly AppDbContext _context;

        public CommentsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(string movieSlug, string content, int? parentCommentID = null)
        {
            var comment = new Comment
            {
                MovieSlug = movieSlug,
                Content = content,
                ParentCommentId = parentCommentID
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsAsync(string movieSlug)
        {
            return await _context.Comments
                .Where(c => c.MovieSlug == movieSlug && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .ToListAsync();
        }
    }
}
