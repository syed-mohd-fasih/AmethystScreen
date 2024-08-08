using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AmethystScreen.Controllers
{
    public class HomeController(AppDbContext appContext, ILogger<HomeController> logger) : Controller
    {
        private readonly AppDbContext _appContext = appContext;
        private readonly ILogger<HomeController> _logger = logger;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult FeedbackSuccess()
        {
            return View();
        }

        public IActionResult SaveFeedback(string title, string description)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                Feedback feedback = new Feedback
                {
                    PostedByUserid = userId,
                    Title = title,
                    Description = description,
                };

                _appContext.Feedbacks.Add(feedback);
                _appContext.SaveChanges();

                return RedirectToAction(nameof(FeedbackSuccess));
            }
            return Error();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError($"{nameof(Error)}: An error occured: Please check previous logs for more info");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
