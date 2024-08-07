using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using AmethystScreen.Services;
using Microsoft.AspNetCore.Authorization;
using AmethystScreen.ViewModels;
using System.Security.Claims;
using AmethystScreen.Areas.Identity.Data;

namespace AmethystScreen.Controllers
{
    [Authorize(Policy = "user")]
    public class LibraryController(AppDbContext context, ILogger<LibraryController> logger, MoviesDirectoryService moviesDirectoryService, CommentsService commentsService, LikesService likesService) : Controller
    {
        private readonly AppDbContext _movieContext = context;
        private readonly ILogger<LibraryController> _logger = logger;
        private readonly MoviesDirectoryService _movieDirectoryService = moviesDirectoryService;
        private readonly CommentsService _commentsService = commentsService;
        private readonly LikesService _likesService = likesService;

        // GET: Library
        public IActionResult Index(string searchTitle, int pageNumber = 1, int pageSize = 8)
        {
            var movies = _movieContext.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                movies = movies.Where(m => m.Title.ToLower().Contains(searchTitle.ToLower()));
            }

            var paginatedMovies = movies
                .OrderBy(m => m.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalMovies = movies.Count();

            var viewModel = new LibraryPagination
            {
                Movies = paginatedMovies,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize)
            };

            return View(viewModel);
        }

        // GET: Library/Movie/<id/slug>
        public async Task<IActionResult> Movie(string slug)
        {
            if (slug == null)
            {
                _logger.LogWarning($"{nameof(Movie)}: null access attempt {slug}");
                return NotFound();
            }

            if (MovieExists(slug))
            {
                var movie = await _movieContext.Movies.FirstOrDefaultAsync(x => x.Slug == slug);
                if (movie == null)
                {
                    _logger.LogError($"{nameof(Movie)}: Movie couldn't be retrieved from context");
                    return RedirectToAction("Error", nameof(HomeController));
                }
                else
                {
                    _logger.LogInformation($"{nameof(Movie)}: {movie.Slug}: was accessed");

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if(userId == null)
                    {
                        _logger.LogWarning($"{nameof(Movie)}: User Id not found");
                        return Unauthorized();
                    }
                    bool hasLiked = await _likesService.IsLiked(slug, userId);

                    var comments = await _commentsService.GetCommentsAsync(slug);
                    var viewModel = new MovieDetails
                    {
                        Movie = movie,
                        Comments = comments,
                        IsLikedByUser = hasLiked
                    };
                    return View(viewModel);
                }
            }
            else
            {
                _logger.LogError($"{nameof(Movie)}: {slug} not found in context");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("/library/video/{*fileName}")]
        public IActionResult Play(string fileName)
        {
            fileName = fileName.Replace("/", "\\");
            var filePath = Path.Combine(MoviesDirectoryService._directory, fileName);
            filePath = Path.GetFullPath(filePath);

            if (System.IO.File.Exists(filePath))
            {
                var fileExtension = Path.GetExtension(filePath);
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                Response.Headers["Accept-Ranges"] = "bytes";
                return File(fileStream, _movieDirectoryService.MimeTypes[fileExtension.ToLowerInvariant()]);
            }

            _logger.LogError($"{nameof(Play)}: {fileName} couldn't be retrieved");
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string movieSlug, string content, int? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning($"{nameof(AddComment)}: Comment cannot be empty");
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Movie", new { slug = movieSlug });
            }

            var movie = await _movieContext.Movies.FirstOrDefaultAsync(m => m.Slug == movieSlug);
            if (movie == null)
            {
                _logger.LogError($"{nameof(AddComment)}: Movie: {movieSlug} not found in context");
                return NotFound();
            }

            string? name = null;
            if(User.Identity != null)
            {
                name = User.Identity.Name;
            }
            name ??= "Anonymous";

            var comment = new Comment
            {
                CommentBy = name,
                MovieSlug = movie.Slug,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _movieContext.Comments.Add(comment);
            await _movieContext.SaveChangesAsync();
            _logger.LogInformation($"{nameof(AddComment)}: Comment added to Db");

            return RedirectToAction("Movie", new { slug = movieSlug });
        }

        [HttpPost]
        public async Task<IActionResult> ReplyTo(string movieSlug, string content, int parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning($"{nameof(ReplyTo)}: Reply cannot be empty");
                TempData["Error"] = "Reply cannot be empty.";
                return RedirectToAction("Movie", new { slug = movieSlug });
            }

            var movie = await _movieContext.Movies.FirstOrDefaultAsync(m => m.Slug == movieSlug);
            if (movie == null)
            {
                _logger.LogError($"{nameof(ReplyTo)}: Movie: {movieSlug} not found in context");
                return NotFound();
            }

            string? name = null;
            if(User.Identity != null)
            {
                name = User.Identity.Name;
            }
            name ??= "Anonymous";

            var parentComment = await _movieContext.Comments.FirstOrDefaultAsync(pc => pc.Id == parentCommentId);

            if (parentComment == null)
            {
                _logger.LogError($"{nameof(ReplyTo)}: Parent comment id:{parentCommentId} not found in context");
                return RedirectToAction("Movie", new { slug = movieSlug });
            }

            var reply = new Comment
            {
                CommentBy = name,
                MovieSlug = movie.Slug,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            parentComment.Replies.Add(reply);

            await _movieContext.SaveChangesAsync();
            _logger.LogInformation($"{nameof(ReplyTo)}: Reply added to Db");

            return RedirectToAction("Movie", new { slug = movieSlug });
        }

        public async Task<IActionResult> LikeMovie(string movieSlug)
        {
            if(movieSlug == null)
            {
                _logger.LogError($"{nameof(LikeMovie)}: Like request on a null movie");
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(LikeMovie)}: userId not found");
                return Unauthorized();
            }

            // Get and Toggle the like status
            var isLiked = !(await _likesService.IsLiked(movieSlug, userId));

            if (isLiked)
            {
                _logger.LogInformation($"{nameof(LikeMovie)}: Like added to {movieSlug}");
                await _likesService.AddLike(movieSlug, userId);
            }
            else
            {
                _logger.LogInformation($"{nameof(LikeMovie)}: Like removed from {movieSlug}");
                await _likesService.RemoveLike(movieSlug, userId);
            }
            return RedirectToAction(nameof(Movie), new { Slug = movieSlug });
        }

        public async Task<IActionResult> SyncMovies()
        {
            _logger.LogInformation($"{nameof(SyncMovies)}: syncing movies");
            await _movieDirectoryService.ImportMoviesFromDirectoryAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReportFromMenu(string movieSlug)
        {
            // get form input
            // get user id
            return View();
        }

        public async Task<IActionResult> ReportFromComments(string movieSlug)
        {
            // get form input
            // get user id
            return View();
        }

        private bool MovieExists(string slug)
        {
            return _movieContext.Movies.Any(e => e.Slug == slug);
        }
    }
}
