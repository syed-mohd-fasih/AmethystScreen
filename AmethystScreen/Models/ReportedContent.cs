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
        public bool IsComments { get; set; }
        public bool IsProfile { get; set; }
    }
}
