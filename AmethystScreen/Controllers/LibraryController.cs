﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using AmethystScreen.Services;
using Microsoft.AspNetCore.Authorization;
using AmethystScreen.ViewModels;
using System.Security.Claims;
using AmethystScreen.Areas.Identity.Data;
using System.Drawing;

namespace AmethystScreen.Controllers
{
    public class LibraryController(AppDbContext context, ILogger<LibraryController> logger, MoviesDirectoryService moviesDirectoryService, CommentsService commentsService, LikesService likesService, WatchListService watchListService) : Controller
    {
        private readonly AppDbContext _movieContext = context;
        private readonly ILogger<LibraryController> _logger = logger;
        private readonly MoviesDirectoryService _movieDirectoryService = moviesDirectoryService;
        private readonly CommentsService _commentsService = commentsService;
        private readonly LikesService _likesService = likesService;
        private readonly WatchListService _watchListService = watchListService;

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
                    var comments = await _commentsService.GetCommentsAsync(slug);
                    if (userId == null)
                    {
                        _logger.LogWarning($"{nameof(Movie)}: User Id not found, visiting user pass");
                        var commentModelTemp = new CommentsModel
                        {
                            currComment = null,
                            AuthenticatedUserId = null
                        };
                        var viewModelTemp = new MovieDetails
                        {
                            Movie = movie,
                            IsLikedByUser = false,
                            IsAddedToWatchListByUser = false,
                            Comments = comments,
                            CommentsModel = commentModelTemp
                        };
                        return View(viewModelTemp);
                    }
                    bool hasLiked = await _likesService.IsLiked(slug, userId);
                    bool hasAddedToWatchList = await _watchListService.IsAddedToList(slug, userId);

                    var commentsModel = new CommentsModel
                    {
                        currComment = null,
                        AuthenticatedUserId = userId
                    };
                    var viewModel = new MovieDetails
                    {
                        Movie = movie,
                        IsLikedByUser = hasLiked,
                        IsAddedToWatchListByUser= hasAddedToWatchList,
                        Comments = comments,
                        CommentsModel = commentsModel
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

        [Authorize(Policy = "user")]
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

        [Authorize(Policy = "user")]
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
            string? id = null;
            if (User.Identity != null)
            {
                name = User.Identity.Name;
                id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            name ??= "Anonymous";
            id ??= "Anonymous";

            await _commentsService.AddCommentAsync(name, id, movieSlug, content, parentCommentId);
            _logger.LogInformation($"{nameof(AddComment)}: Comment added to Db");

            return RedirectToAction("Movie", new { slug = movieSlug });
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> LikeMovie(string movieSlug)
        {
            if (movieSlug == null)
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
            return RedirectToAction(nameof(Movie), new { slug = movieSlug });
        }

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> AddToWatchList(string movieSlug)
        {
            if (movieSlug == null)
            {
                _logger.LogError($"{nameof(AddToWatchList)}: Add to watch list request on a null movie");
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(AddToWatchList)}: userId not found");
                return Unauthorized();
            }

            // Get and Toggle the like status
            var isAdded = !(await _watchListService.IsAddedToList(movieSlug, userId));

            if (isAdded)
            {
                _logger.LogInformation($"{nameof(AddToWatchList)}: added to watchlist {movieSlug}");
                await _watchListService.AddToList(movieSlug, userId);
            }
            else
            {
                _logger.LogInformation($"{nameof(AddToWatchList)}: removed from watchlist {movieSlug}");
                await _watchListService.RemoveFromList(movieSlug, userId);
            }
            return RedirectToAction(nameof(Movie), new { slug = movieSlug });
        }

        [Authorize(Policy = "user")]
        public async Task<IActionResult> LikedMovies()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(LikeMovie)}: userId not found");
                return Unauthorized();
            }

            List<Like> Likes = await _movieContext.Likes.Where(l => l.UserId == userId).ToListAsync();
            List<Movie> LikedMoviesList = new();
            foreach(var like in Likes)
            {
                var movie = await _movieContext.Movies.FirstOrDefaultAsync(m => m.Slug == like.MovieSlug);

                if (movie == null)
                {
                    _logger.LogError($"{nameof(LikedMovies)}: Movie {like.MovieSlug} not found in context");
                    continue;
                }

                LikedMoviesList.Add(movie);
            }
            return View(LikedMoviesList);
        }

        [Authorize(Policy = "user")]
        public async Task<IActionResult> ToWatchList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(ToWatchList)}: userId not found");
                return Unauthorized();
            }

            List<ToWatch> toWatch = await _movieContext.ToWatch.Where(l => l.UserId == userId).ToListAsync();
            List<Movie> WatchList = new();
            foreach(var watch in toWatch)
            {
                var movie = await _movieContext.Movies.FirstOrDefaultAsync(m => m.Slug == watch.MovieSlug);

                if (movie == null)
                {
                    _logger.LogError($"{nameof(ToWatchList)}: Movie {watch.MovieSlug} not found in context");
                    continue;
                }

                WatchList.Add(movie);
            }
            return View(WatchList);
        }

        [Authorize(Policy = "user")]
        public async Task<IActionResult> DeleteComment(int commentId, string movieSlug)
        {
            var comment = await _movieContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                _logger.LogError($"{nameof(DeleteComment)}: comment: {commentId} not found in db");
                return NotFound();
            }

            if (comment.CommentByUserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogInformation($"{nameof(DeleteComment)}: removing comment {commentId} from db...");
                await _commentsService.RemoveCommentAsync(commentId);
            }
            else
            {
                _logger.LogWarning($"{nameof(DeleteComment)}: User not author of comment to remove");
            }
            return RedirectToAction(nameof(Movie), new { slug = movieSlug });
        }

        public async Task<IActionResult> SyncMovies()
        {
            _logger.LogInformation($"{nameof(SyncMovies)}: syncing movies");
            await _movieDirectoryService.ImportMoviesFromDirectoryAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "user")]
        public IActionResult ReportUser(string reportedUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(ReportMovie)}: User not found");
                return NotFound();
            }

            var date = DateTime.UtcNow;

            ReportedContent report = new ReportedContent
            {
                ReportedByUserId = userId,

                // Title and Description yet to get; will get on review page:
                Title = string.Empty,
                Description = string.Empty,

                ReportDate = date,
                IsResolved = false,

                IsUser = true,
                ReportedUserId = reportedUserId,

                IsMovie = false,
                ReportedMovieSlug = string.Empty,

                IsComments = false,
                ReportedCommentId = -1,

                ReportStatus = ReportedContent.Status.Pending
            };

            return RedirectToAction(nameof(Report), report);
        }

        [Authorize(Policy = "user")]
        public IActionResult ReportMovie(string reportedMovieSlug)
        {
            if(string.IsNullOrEmpty(reportedMovieSlug))
            {
                _logger.LogWarning($"{nameof(ReportMovie)}: Null movie slug.");
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                _logger.LogError($"{nameof(ReportMovie)}: User not found");
                return NotFound();
            }

            var date = DateTime.UtcNow;
            
            ReportedContent report = new ReportedContent
            {
                ReportedByUserId = userId,

                // Title and Description yet to get; will get on review page:
                Title = string.Empty,
                Description = string.Empty,

                ReportDate = date,
                IsResolved = false,
                
                IsUser = false,
                ReportedUserId = string.Empty,

                IsMovie = true,
                ReportedMovieSlug = reportedMovieSlug,

                IsComments = false,
                ReportedCommentId = -1,

                ReportStatus = ReportedContent.Status.Pending
            };

            return RedirectToAction(nameof(Report), report);
        }

        [Authorize(Policy = "user")]
        public IActionResult ReportComment(int reportedCommentId, string reportedMovieSlug)
        {
            if (string.IsNullOrEmpty(reportedMovieSlug) || reportedCommentId <= 0 )
            {
                _logger.LogWarning($"{nameof(ReportMovie)}: Null movie slug {reportedMovieSlug} or invalid commentId {reportedCommentId}");
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogError($"{nameof(ReportMovie)}: User not found");
                return NotFound();
            }

            var date = DateTime.UtcNow;

            ReportedContent report = new ReportedContent
            {
                ReportedByUserId = userId,

                // Title and Description yet to get; will get on review page:
                Title = string.Empty,
                Description = string.Empty,

                ReportDate = date,
                IsResolved = false,

                IsUser = false,
                ReportedUserId = string.Empty,

                IsMovie = false,
                ReportedMovieSlug = reportedMovieSlug,

                IsComments = true,
                ReportedCommentId = reportedCommentId,

                ReportStatus = ReportedContent.Status.Pending
            };

            return RedirectToAction(nameof(Report), report);
        }

        [Authorize(Policy = "user")]
        public IActionResult Report(ReportedContent reportedContent)
        {
            // Action used to funnel the above 3 methods to display single form with different data
            return View(reportedContent);
        }

        [Authorize(Policy = "user")]
        public async Task<IActionResult> CreateReport(string title, string description, bool isUser = false, string reportedUserId = "", bool isMovie = false, string reportedMovieSlug = "", bool isComments = false, int reportedCommentId = -1)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                _logger.LogError($"{nameof(CreateReport)}: empty title or description...");
                return BadRequest();
            }
            if (!isUser && !isMovie && !isComments)
            {
                _logger.LogError($"{nameof(CreateReport)}: invalid report creating request\n {isUser}, {isMovie}, {isComments}");
                return BadRequest();
            }
            if (isUser && string.IsNullOrEmpty(reportedUserId))
            {
                _logger.LogError($"{nameof(CreateReport)}: invalid user report.. {reportedUserId}");
                return BadRequest();
            }
            if (isMovie && string.IsNullOrEmpty(reportedMovieSlug))
            {
                _logger.LogError($"{nameof(CreateReport)}: invalid movie report.. {reportedMovieSlug}");
                return BadRequest();
            }
            if (isComments && string.IsNullOrEmpty(reportedMovieSlug) && reportedCommentId <= 0)
            {
                _logger.LogError($"{nameof(CreateReport)}: invalid comment report.. {reportedMovieSlug}, {reportedCommentId}");
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                _logger.LogError($"{nameof(CreateReport)}: User not found");
                return NotFound();
            }
            var reportDate = DateTime.UtcNow;

            ReportedContent report = new ReportedContent
            {
                ReportedByUserId = userId,
                Title = title,
                Description = description,
                ReportDate = reportDate,
                IsResolved = false,
                IsUser = isUser,
                ReportedUserId = reportedUserId,
                IsMovie = isMovie,
                ReportedMovieSlug = reportedMovieSlug,
                IsComments = isComments,
                ReportedCommentId = reportedCommentId,
                ReportStatus = ReportedContent.Status.Pending
            };
            
            _movieContext.ReportedContent.Add(report);
            await _movieContext.SaveChangesAsync();
            _logger.LogInformation($"{nameof(CreateReport)}: Report saved to db");

            return RedirectToAction(nameof(Movie), new { slug = reportedMovieSlug });
        }

        private bool MovieExists(string slug)
        {
            return _movieContext.Movies.Any(e => e.Slug == slug);
        }

        [HttpGet]
        public JsonResult CheckAuthorization()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            return Json(new { isAuthenticated });
        }
    }
}
