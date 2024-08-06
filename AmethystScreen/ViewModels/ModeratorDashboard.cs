using AmethystScreen.Areas.Identity.Data;
using AmethystScreen.Models;

namespace AmethystScreen.ViewModels
{
    public class ModeratorDashboard
    {
        public IEnumerable<User>? Users { get; set; }
        public IEnumerable<ReportedContent>? ReportedContent { get; set; }
        public IEnumerable<ContentActivity>? RecentActivity { get; set; }
        public IEnumerable<Feedback>? Feedbacks { get; set; }
    }
}
