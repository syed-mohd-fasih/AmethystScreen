using System.Text.Json.Serialization;

namespace AmethystScreen.Models
{
    public class Movie
    {
        // For Db and page redirects
        public int Id { get; set; }
        public int Year { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("img")]
        public string Image { get; set; } = string.Empty;
        public double Rating { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Likes { get; set; }
    }
}
