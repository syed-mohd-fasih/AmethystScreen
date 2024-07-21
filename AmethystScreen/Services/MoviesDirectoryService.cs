using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace AmethystScreen.Services
{
    // Redo From StreamSpot
    public class MoviesDirectoryService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;
        public readonly string _directory = Path.Combine("D:", "Movies");
        public readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { ".mp4", "video/mp4" },
            { ".webm", "video/webm" },
            { ".ogv", "video/ogg" },
            { ".avi", "video/x-msvideo" },
            { ".mov", "video/quicktime" },
            { ".wmv", "video/x-ms-wmv" },
            { ".flv", "video/x-flv" },
            { ".mkv", "video/x-matroska" },
            { ".mpeg", "video/mpeg" }
        };

        public async Task<IEnumerable<Movie>> GetMoviesAsync()
        {
            return await _context.Movies.ToListAsync();
        }

        private async Task AddMovieAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        public async Task ImportMoviesFromDirectoryAsync()
        {
            var directories = Directory.GetDirectories(_directory);

            foreach (var d in directories)
            {
                var jsonFilePath = Path.Combine(d, "movie.json");
                if (File.Exists(jsonFilePath))
                {
                    string json = File.ReadAllText(jsonFilePath);
                    JsonSerializerOptions jsonSerializerOptions = new()
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                    };
                    JsonSerializerOptions options = jsonSerializerOptions;
                    var movie = JsonSerializer.Deserialize<Movie>(json, options);
                    if (movie != null)
                    {
                        string? url = FileVideoUrl(d);
                        if (url == null) movie.VideoUrl = "<Not Found>";
                        else movie.VideoUrl = url;
                        await AddMovieAsync(movie);
                    }
                }
            }
        }

        private static string? FileVideoUrl(string d)
        {
            string[] videoFormats = [".mp4", ".mov", ".mkv", ".webm", ".avi", ".wmv", ".mpeg", ".mpg"];

            foreach (string ext in videoFormats)
            {
                string[] files = Directory.GetFiles(d, "*" + ext);
                if (files.Length > 0)
                {
                    return files[0];
                }
            }

            return null;
        }
    }
}
