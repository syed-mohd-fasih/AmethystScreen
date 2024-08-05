using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.AspNetCore.Authorization;
using AmethystScreen.Services;
using System.Security.Claims;

namespace AmethystScreen.Controllers
{
    [Authorize(Policy = "moderator")]
    public class ModController : Controller
    {
        private readonly AppDbContext _appContext;
        private readonly UserDbContext _userContext;
        private readonly RolesService _rolesService;


        public ModController(AppDbContext appContext, UserDbContext userContext, RolesService rolesService)
        {
            _appContext = appContext;
            _userContext = userContext;
            _rolesService = rolesService;
        }

        // GET: Mod
        public async Task<IActionResult> Index()
        {
            return View(await _rolesService.GetUsersAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        }

        // GET: Mod/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userContext.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Mod/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: Mod/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Year,Title,Tags,Description,Image,Rating,VideoUrl,Language,Slug,Likes")] Movie movie)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(movie);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(movie);
        //}

        // GET: Mod/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _appContext.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Mod/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Year,Title,Tags,Description,Image,Rating,VideoUrl,Language,Slug,Likes")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _appContext.Update(movie);
                    await _appContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Mod/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _appContext.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Mod/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _appContext.Movies.FindAsync(id);
            if (movie != null)
            {
                _appContext.Movies.Remove(movie);
            }

            await _appContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _appContext.Movies.Any(e => e.Id == id);
        }
    }
}
