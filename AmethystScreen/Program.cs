using AmethystScreen.Data;
using AmethystScreen.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AmethystScreen.Areas.Identity.Data;

namespace AmethystScreen
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("AmethystMovieContext")));
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("AmethystUserContext")));
            builder.Services.AddDefaultIdentity<User>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddScoped<MoviesDirectoryService>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Request URL: {URL}", context.Request.Path);
                await next.Invoke();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.Migrate();

                // To seed Db
                if (context.Movies.Count() <= 0)
                {
                    var movieService = scope.ServiceProvider.GetRequiredService<MoviesDirectoryService>();
                    await movieService.ImportMoviesFromDirectoryAsync();
                }
            }

            app.Run();
        }
    }
}
