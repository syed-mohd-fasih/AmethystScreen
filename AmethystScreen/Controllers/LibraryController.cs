using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using AmethystScreen.Services;
using Microsoft.AspNetCore.Authorization;
using AmethystScreen.ViewModels;

namespace AmethystScreen.Controllers
{
    [Authorize]
    public class LibraryController(AppDbContext context, ILogger<LibraryController> logger, MoviesDirectoryService moviesDirectoryService, CommentsService commentsService) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<LibraryController> _logger = logger;
        private readonly MoviesDirectoryService _movieDirectoryService = moviesDirectoryService;
        private readonly CommentsService _commentsService = commentsService;

        // GET: Library
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"{nameof(Index)}: getting movies to controller");
            var movies = await _movieDirectoryService.GetMoviesAsync();
            return View(movies);
        }

        // GET: Library/Movie/<id>
        public async Task<IActionResult> Movie(string slug)
        {
            if (slug == null)
            {
                _logger.LogWarning($"{nameof(Movie)}: null access attempt {slug}");
                return NotFound();
            }

            if (MovieExists(slug))
            {
                var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Slug == slug);
                if (movie == null)
                {
                    _logger.LogError($"{nameof(Movie)}: Movie couldn't be retrieved from context");
                    return RedirectToAction("Error", nameof(HomeController));
                }
                else
                {
                    _logger.LogInformation($"{nameof(Movie)}: {movie.Slug}: was accessed");
                    var comments = await _commentsService.GetCommentsAsync(slug);
                    var viewModel = new MovieDetails
                    {
                        Movie = movie,
                        Comments = comments
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

        // GET: Library/SearchResult
        public IActionResult SearchResult(string SearchTitle)
        {
            var movieList = _context.Movies.Where(m => m.Title.Contains(SearchTitle)).ToList();

            if (movieList.Count == 0)
            {
                return View(nameof(Index), movieList);
            }
            else
            {
                movieList = (List<Movie>)[];
                return RedirectToAction(nameof(Index), movieList);
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
                return File(fileStream, _movieDirectoryService.MimeTypes[fileExtension.ToLowerInvariant()]);
            }

            _logger.LogError($"{nameof(Play)}: {fileName} couldn't be retrieved");
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string movieSlug, string content, int? parentCommentId = null)
        {
            Console.WriteLine($"==================={movieSlug} {content}===================");
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning($"{nameof(AddComment)}: Comment cannot be empty");
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Movie", new { Slug = movieSlug });
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Slug == movieSlug);
            if (movie == null)
            {
                _logger.LogError($"{nameof(AddComment)}: {movieSlug} not found in context");
                return NotFound();
            }

            string? name = User.Identity.Name;
            name ??= "Anonymous";


            var comment = new Comment
            {
                CommentBy = name,
                MovieSlug = movie.Slug,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{nameof(AddComment)}: Comment added to Db");

            return RedirectToAction("Movie", new { Slug = movieSlug });
        }

        public async Task<IActionResult> SyncMovies()
        {
            _logger.LogInformation($"{nameof(SyncMovies)}: syncing movies");
            await _movieDirectoryService.ImportMoviesFromDirectoryAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(string slug)
        {
            return _context.Movies.Any(e => e.Slug == slug);
        }
    }
}
