using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;

namespace AmethystScreen.Services
{
    public class CommentsService
    {
        private readonly AppDbContext _context;
        private ILogger<CommentsService> _logger;

        public CommentsService(AppDbContext context, ILogger<CommentsService> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<List<Comment>> GetCommentsAsync(string movieSlug)
        {
            return await _context.Comments
                .Where(c => c.MovieSlug == movieSlug && c.ParentCommentId == null)
                .Include(c => c.Replies)
                .ToListAsync();
        }

        public async Task<bool> RemoveCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                _logger.LogWarning($"{nameof(RemoveCommentAsync)}: comment {commentId} not found in db");
                return false;
            }

            var commentReplies = await _context.Comments.Where(r => r.ParentCommentId ==  commentId).ToListAsync();

            if(commentReplies.Any())
            {
                _logger.LogInformation($"{nameof(RemoveCommentAsync)}: Removing replies from db");
                foreach(var reply in commentReplies)
                {
                    _context.Comments.Remove(reply);
                }
            }

            _logger.LogInformation($"{RemoveCommentAsync}: Removing {commentId} from db");
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
