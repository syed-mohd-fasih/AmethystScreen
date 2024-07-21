using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using AmethystScreen.Services;

namespace AmethystScreen.Controllers
{
    public class LibraryController(AppDbContext context, ILogger<LibraryController> logger, MoviesDirectoryService moviesDirectoryService) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<LibraryController> _logger = logger;
        private readonly MoviesDirectoryService _movieDirectoryService = moviesDirectoryService;

        // GET: Library
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"{nameof(Index)}: getting movies to controller");
            var movies = await _movieDirectoryService.GetMoviesAsync();
            return View(movies);
        }

        // GET: Library/Movie/<id>
        public async Task<IActionResult> Movie(int id)
        {
            if (id == 0)
            {
                _logger.LogWarning($"{nameof(Movie)}: ID:0 access attempt");
                return NotFound();
            }

            if (MovieExists(id))
            {
                var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
                if (movie == null)
                {
                    _logger.LogError($"{nameof(Movie)}: Movie couldn't be retrieved from context");
                    return RedirectToAction("Error", nameof(HomeController));
                }
                else
                {
                    _logger.LogInformation($"{nameof(Movie)}: {movie.Id}:{movie.Title} was accessed");
                    return View(movie);
                }
            }
            else
            {
                _logger.LogError($"{nameof(Movie)}: Movie {id} not found in context");
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
            var filePath = Path.Combine(_movieDirectoryService._directory, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileExtension = Path.GetExtension(filePath);
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, _movieDirectoryService.MimeTypes[fileExtension.ToLowerInvariant()]);
            }

            _logger.LogError($"{nameof(Play)}: {fileName} couldn't be retrieved");
            return NotFound();
        }

        public async Task<IActionResult> SyncMovies()
        {
            _logger.LogInformation($"{nameof(SyncMovies)}: syncing movies");
            await _movieDirectoryService.ImportMoviesFromDirectoryAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
