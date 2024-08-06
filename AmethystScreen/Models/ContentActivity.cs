using System.Drawing;

namespace AmethystScreen.Models
{
    public class ContentActivity
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string ActingRoleId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime ContentActedUponAt { get; set; }
    }
}
