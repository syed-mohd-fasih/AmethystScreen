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
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("superuser", policy => policy.RequireRole("SuperUser"));
                options.AddPolicy("admin", policy => policy.RequireRole("SuperUser", "Admin"));
                options.AddPolicy("moderator", policy => policy.RequireRole("SuperUser", "Admin", "Moderator"));
                options.AddPolicy("user", policy => policy.RequireRole("SuperUser", "Admin", "Moderator", "User"));
            });

            builder.Services.AddScoped<MoviesDirectoryService>();
            builder.Services.AddScoped<CommentsService>();
            builder.Services.AddScoped<LikesService>();

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
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                context.Database.Migrate();

                // To seed Db
                if (context.Movies.Count() <= 0)
                {
                    var movieService = scope.ServiceProvider.GetRequiredService<MoviesDirectoryService>();
                    await movieService.ImportMoviesFromDirectoryAsync();
                }

                // To seed users
                await RoleSeeder.SeedRolesAsync(roleManager);
            }

            app.Run();
        }
    }

    public class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "SuperUser", "Admin", "User", "Moderator" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
