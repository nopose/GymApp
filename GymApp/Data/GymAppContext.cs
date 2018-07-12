
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymApp.Models;

namespace GymApp.Data
{
    public class GymAppContext : IdentityDbContext<AppUser>
    {

        public DbSet<TrainingProgram> Workouts { get; set; }
        public DbSet<ProgramExercises> PExercises { get; set; }
        public DbSet<ExerciseSets> ESets { get; set; }
        public DbSet<UserExercise> UserEx { get; set; }

        public GymAppContext(DbContextOptions<GymAppContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TrainingProgram>().ToTable("Workouts");
            builder.Entity<UserExercise>().ToTable("UserEx");
            builder.Entity<ProgramExercises>().ToTable("PExercises");
            builder.Entity<ExerciseSets>().ToTable("ESets");

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
