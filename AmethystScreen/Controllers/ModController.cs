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
using AmethystScreen.ViewModels;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = await _rolesService.GetAllUsersAsync(userId);
            var reportedContent = await _rolesService.GetReportedContentAsync(userId);
            var recentActivity = await _rolesService.GetRecentActivityAsync();
            var feedbacks = await _rolesService.GetFeedbackAsync();

            ModeratorDashboard modDash = new ModeratorDashboard
            {
                Users = users,
                ReportedContent = reportedContent,
                RecentActivity = recentActivity,
                Feedbacks = feedbacks
            };

            return View(modDash);
        }

        // GET: Mod/ReviewReport/xxxx-xxxx-xxxx // probably change to reports
        public async Task<IActionResult> ReviewReport(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var report = await _appContext.ReportedContent
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        public async Task<IActionResult> ReportAction(int? id, bool action)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            var report = await _appContext.ReportedContent.FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                //log for error: report not found
                return NotFound();
            }

            if (action)
            {
                report.ReportStatus = ReportedContent.Status.UnderReview;
            }
            else
            {
                report.ReportStatus = ReportedContent.Status.Rejected;
            }

            await _appContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Mod/ManageUser/xxxx-xxxx-xxxx
        public async Task<IActionResult> ManageUser(string userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            var user = await _userContext.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Mod/ReviewFeedback/xxxx-xxxx-xxxx
        public async Task<IActionResult> ReviewFeedback(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _appContext.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }
    }
}
