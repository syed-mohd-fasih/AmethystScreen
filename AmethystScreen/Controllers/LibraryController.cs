using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using System.Drawing;
using AmethystScreen.Services;

namespace AmethystScreen.Controllers
{
    public class LibraryController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly MoviesDirectoryService _movieDirectoryService = new(context);
        
        // GET: Library
        public async Task<IActionResult> Index()
        {
            var movies = await _movieDirectoryService.GetMoviesAsync();
            return View(movies);
        }

        // GET: Library/Movie/<id>
        public async Task<IActionResult> Movie(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (MovieExists(id))
            {
                var movie = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
                return View(movie);
            }
            else
            {
                // Change for future custom error page
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Library/SearchResult
        public IActionResult SearchResult(string SearchTitle)
        {
            //if (SearchTitle == null || SearchTitle == String.Empty || SearchTitle == "")
            //{
            //    return NotFound();
            //}

            var movieList = _context.Movies.Where(m => m.Title.Contains(SearchTitle)).ToList();

            if (movieList.Count == 0)
            {
                return View(nameof(Index), movieList);
            }
            else
            {
                movieList = (List<Movie>)[];
                // Change for future custom error page
                return RedirectToAction(nameof(Index), movieList);
            }
        }

        public async Task<IActionResult> SyncMovies()
        {
            await _movieDirectoryService.ImportMoviesFromDirectoryAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
