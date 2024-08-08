using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;

namespace AmethystScreen.Services
{
    public class CommentsService
    {
        private readonly AppDbContext _context;

        public CommentsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(string name, string userId, string movieSlug, string content, int? parentCommentId = null)
        {
            var comment = new Comment
            {
                CommentByUsername = name,
                CommentByUserId = userId,
                MovieSlug = movieSlug,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task AddReplyAsync(string name, string userId, string movieSlug, string content, int? parentCommentId, Comment parentComment)
        {
            var reply = new Comment
            {
                CommentByUsername = name,
                CommentByUserId = userId,
                MovieSlug = movieSlug,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            parentComment.Replies.Add(reply);
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
