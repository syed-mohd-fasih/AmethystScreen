using System.Drawing;

namespace AmethystScreen.Models
{
    public class ReportedContent
    {
        public int Id { get; set; }

        // App reports (add to feedback class?)
        public string ReportedByUserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public bool IsResolved { get; set; }

        // User Specific Reports
        public bool IsUser { get; set; }
        public string ReportedUserId { get; set; } = string.Empty;


        // Video/Movie Reports
        public bool IsMovie { get; set; }
        public string ReportedMovieSlug { get; set; } = string.Empty;

        // Comment Reports
        public bool IsComments { get; set; }
        public int ReportedCommentId { get; set; }
        
        public enum Status
        {
            Pending,
            UnderReview,
            Approved,
            Rejected
        };

        public Status ReportStatus { get; set; }
    }
}
