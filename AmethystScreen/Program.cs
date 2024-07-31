using AmethystScreen.Data;
using AmethystScreen.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AmethystScreen.Areas.Identity.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AmethystScreen
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("AmethystMovieContext")));
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("AmethystUserContext")));
            
            builder.Services.AddIdentity<User, IdentityRole>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                })
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperUserPolicy", policy => policy.RequireRole("SuperUser"));
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin", "SuperUser"));
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "SuperUser"));
                options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole("Moderator", "SuperUser"));
            });
            builder.Services.AddAuthentication();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
            
            builder.Services.AddScoped<MoviesDirectoryService>();
            builder.Services.AddScoped<CommentsService>();
            builder.Services.AddScoped<LikesService>();
            builder.Services.AddSingleton<IEmailSender, MockEmailSenderService>();

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
                var appContext = services.GetRequiredService<AppDbContext>();
                var userContext = services.GetRequiredService<UserDbContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<User>>();

                appContext.Database.Migrate();
                userContext.Database.Migrate();

                string[] roles = { "SuperUser", "Admin", "User", "Moderator" };
                foreach(var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // To seed Db
                if (appContext.Movies.Count() <= 0)
                {
                    var movieService = scope.ServiceProvider.GetRequiredService<MoviesDirectoryService>();
                    await movieService.ImportMoviesFromDirectoryAsync();
                }
            }

            app.Run();
        }
    }
}
    