﻿using AmethystScreen.Models;
using Microsoft.EntityFrameworkCore;
namespace AmethystScreen.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<ReportedContent> ReportedContent { get; set; }
        public DbSet<ToWatch> ToWatch { get; set; }
    }
}
